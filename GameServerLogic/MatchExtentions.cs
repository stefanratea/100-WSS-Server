using System;
using System.Reflection;

namespace WebSocketServer.GameServerLogic
{
    public static class MatchExtentions
    {
        private static Object GetFieldValue(this Object obj, String name)
        {
                if (obj == null) { return null; }

                Type type = obj.GetType();
                FieldInfo info = type.GetField(name);
                if (info == null) { Console.Write("???????"); return null; }

                obj = info.GetValue(obj);
            return obj;
        }

        public static T GetFieldValue<T>(this Object obj, String name)
        {
            Object retval = GetFieldValue(obj, name);
            if (retval == null) { Console.Write("#########"); return default(T); }

            // throws InvalidCastException if types are incompatible
            return (T)retval;
        }
    }
}
