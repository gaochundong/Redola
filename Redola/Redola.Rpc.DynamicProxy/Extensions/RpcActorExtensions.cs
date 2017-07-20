namespace Redola.Rpc.DynamicProxy
{
    public static class RpcActorExtensions
    {
        public static void Register<TService>(this RpcActor localActor, TService service)
        {
            var localService = RpcServiceGenerator.CreateService<TService>(localActor, service);
            localActor.RegisterRpcService(localService as RpcService);
        }

        public static TService Resolve<TService>(this RpcActor localActor, string service)
        {
            var remoteService = RpcServiceProxyGenerator.CreateServiceProxy<TService>(localActor, service);
            localActor.RegisterRpcService(remoteService as RpcService);
            return remoteService;
        }
    }
}
