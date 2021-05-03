using System;

namespace LOIN.Server.Swagger
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    sealed class EnableLoinContextAttribute : Attribute
    {
        public EnableLoinContextAttribute()
        {
        }
    }
}
