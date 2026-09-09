using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Vehiculo.Api.Data;
using Vehiculo.Api.Events;
using Vehiculo.Api.Models;

namespace Vehiculo.Api.Services
{
    public class RabbitMQConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<RabbitMQConsumer> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private IConnection? _connection;
        private IChannel? _channel;

        public RabbitMQConsumer(
            IConfiguration configuration,
            ILogger<RabbitMQConsumer> logger,
            IServiceScopeFactory scopeFactory)
        {
            _configuration = configuration;
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:HostName"],
                Port = int.Parse(_configuration["RabbitMQ:Port"]!),
                UserName = _configuration["RabbitMQ:UserName"],
                Password = _configuration["RabbitMQ:Password"]
            };

            // Bucle de reintento para esperar a que RabbitMQ esté completamente disponible
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _connection = await factory.CreateConnectionAsync();
                    _channel = await _connection.CreateChannelAsync();
                    _logger.LogInformation("✅ Conexión establecida exitosamente con RabbitMQ en {HostName}", factory.HostName);
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("⏳ Esperando conexión a RabbitMQ en {HostName}... Reintentando en 3s. Detalle: {Message}", factory.HostName, ex.Message);
                    await Task.Delay(3000, stoppingToken);
                }
            }

            if (stoppingToken.IsCancellationRequested || _channel == null) return;

            var queueName = _configuration["RabbitMQ:QueueName"]!;

            await _channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (sender, ea) =>
            {
                var body = ea.Body.ToArray();
                var mensaje = Encoding.UTF8.GetString(body);

                var evento = JsonSerializer.Deserialize<CategoriaCreadaEvento>(mensaje);

                if (evento != null)
                {
                    _logger.LogInformation(
                        "📢 [EVENTO RECIBIDO EN VEHICULO.API] -> Se ha creado una nueva Categoría: [ID: {IdCategoria}] Nombre: '{Nombre}'",
                        evento.IdCategoria,
                        evento.Nombre
                    );

                    using var scope = _scopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<VehiculoDBContext>();

                    // Verificar si ya existe un vehículo para esta categoría o crear uno inicial
                    var existe = await dbContext.Vehiculos.AnyAsync(v => v.IdCategoria == evento.IdCategoria);

                    if (!existe)
                    {
                        var nuevoVehiculo = new Models.Vehiculo
                        {
                            IdCategoria = evento.IdCategoria,
                            Marca = "Por definir",
                            Modelo = "Modelo Base (" + evento.Nombre + ")",
                            Precio = 0.00m,
                            Stock = 0,
                            Estado = true
                        };

                        dbContext.Vehiculos.Add(nuevoVehiculo);
                        await dbContext.SaveChangesAsync();

                        _logger.LogInformation(
                            "🚗 [AUTO-CREACIÓN] -> Vehículo inicial creado automáticamente en base de datos para la Categoría ID: {IdCategoria}",
                            evento.IdCategoria
                        );
                    }
                }

                await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
            };

            await _channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: false,
                consumer: consumer
            );

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}