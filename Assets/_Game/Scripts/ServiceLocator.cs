namespace Game
{
    using System;
    using System.Collections.Generic;

    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> services = new();

        public static void Register<T>(T service)
        {
            services[typeof(T)] = service;
        }

        public static void Unregister<T>()
        {
            services.Remove(typeof(T));
        }

        public static bool TryGet<T>(out T service)
        {
            if (services.TryGetValue(typeof(T), out var obj))
            {
                service = (T)obj;
                return true;
            }

            service = default;
            return false;
        }
    }
}