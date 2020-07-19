namespace DotNet.Net.MQTT
{
    /// <summary>
    /// 一个简单的mqtt 服务器。
    /// </summary>
    public class MQTTServer : TcpServer<MQTTSocketClient, MQTTDataPackage>
    {
        /// <summary>
        /// 当有新的客户端连接到服务器时发生。
        /// </summary>
        /// <param name="client"></param>
        protected override void OnNewClient(MQTTSocketClient client)
        {
            client.TcpServer = this;
            base.OnNewClient(client);

        }
        /// <summary>
        /// 根据客户端编号获取客户端连接
        /// </summary>
        /// <param name="clientId">客户端编号。</param>
        /// <returns></returns>
        public virtual Result<MQTTSocketClient> GetClientById(string clientId)
        {
            foreach (var client in Clients)
            {
                if (client.ClientId == clientId)
                {
                    return client;
                }
            }
            return new Result<MQTTSocketClient>(false);
        }
    }
}
