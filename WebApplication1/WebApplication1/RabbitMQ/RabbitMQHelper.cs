using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using WebApplication1.Models;

namespace WebApplication1.RabbitMQ
{
    /// <summary>
    /// RabbitMQHelper
    /// </summary>
    public class RabbitMQHelper
    {
        private static readonly ConnectionFactory _connectionFactory = null;
        private static IConnection connection = null;
        private static IModel channel = null;


        /// <summary>
        /// 构造函数
        /// </summary>
        static RabbitMQHelper()
        {
            //初始化MQ基本设置
            _connectionFactory = new ConnectionFactory();
            //设置MQ主机地址
            _connectionFactory.HostName = "localhost";
            //设置MQ用户名
            _connectionFactory.UserName = "guest";
            //设置MQ密码
            _connectionFactory.Password = "guest";
            //设置MQ发生意外断开后自动重连
            _connectionFactory.AutomaticRecoveryEnabled = true;

            //创建connection和channel提供长时监听
            connection = _connectionFactory.CreateConnection();
            channel = connection.CreateModel();

            //重新加载RabbitMQHelper类后会删除指定路由器和队列
            //这会导致如果之前还有处于挂起的队列没有处理完，将丢弃这些没有处理完的队列
            channel.ExchangeDelete("testExchange");
            channel.QueueDelete("testQueue");
        }

        #region 单消息入队

        /// <summary>
        /// 单消息入队
        /// </summary>
        /// <param name="exchangeName">交换器名称</param>
        /// <param name="queueName">路由名称</param>
        /// <param name="message">消息实例</param>
        public static void Enqueue<T>(string exchangeName, string queueName, T message)
        {
            try
            {
                if (message != null)
                {
                    using (IConnection connection = _connectionFactory.CreateConnection())
                    {
                        using (IModel channel = connection.CreateModel())
                        {
                            //定义路由器及路由规则
                            //设置路由规则为direct，将消息路由到指定routingKey的队列上
                            channel.ExchangeDeclare(exchangeName, "direct", durable: true, autoDelete: false,
                                arguments: null);
                            //定义队列
                            channel.QueueDeclare(queueName, durable: true, autoDelete: false, exclusive: false,
                                arguments: null);
                            //绑定队列和路由器，并设置routingKey
                            channel.QueueBind(queueName, exchangeName, routingKey: queueName);
                            string messageString = JsonConvert.SerializeObject(message);
                            byte[] body = Encoding.UTF8.GetBytes(messageString);
                            //channel其他属性配置
                            var properties = channel.CreateBasicProperties();
                            //使消息持久化
                            properties.Persistent = true;
                            //发送消息，路由器将采用以上配置进行路由
                            //此处路由规则为direct，将消息路由到指定routingKey的队列上
                            channel.BasicPublish(exchangeName, queueName, properties, body);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion

        #region 消费消息队列

        /// <summary>
        /// 消费消息队列
        /// </summary>
        /// <typeparam name="T">消息对象</typeparam>
        /// <param name="exchangeName">交换器名称</param>
        /// <param name="queueName">队列名称</param>
        /// <param name="func">消费消息的具体操作</param>
        public static void Listening<T>(string exchangeName, string queueName, Func<T, bool> func)
        {
            try
            {
                //定义队列
                channel.QueueDeclare(queueName, durable: true, autoDelete: false, exclusive: false, arguments: null);
                EventingBasicConsumer consumer = new EventingBasicConsumer(channel);
                //队列消费事件（队列收到消息后自动进行消费）
                consumer.Received += (sender, eventArgs) =>
                {
                    byte[] body = eventArgs.Body;
                    if (body != null && body.Length > 0)
                    {
                        string message = Encoding.UTF8.GetString(body);
                        if (!string.IsNullOrWhiteSpace(message))
                        {
                            T queueMessage = JsonConvert.DeserializeObject<T>(message);
                            if (queueMessage != null)
                            {

                                //消费消息
                                var result = func(queueMessage);
                                
                                //成功消费后通知队列销毁消息（避免重复消费）
                                channel.BasicAck(eventArgs.DeliveryTag, false);
                            }
                        }
                    }
                };
                //设置noAck为false，暂时不删除消息，等待ack通知后再删除
                channel.BasicConsume(queueName, false, consumer);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion


        
        
    }
}