using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace MicroIOC
{
    public class IOC
    {
        public delegate object GeneratorFunc();        
        private static Dictionary<Type, GeneratorFunc> resolvers = new Dictionary<Type, GeneratorFunc>();

        public static void Register<T>(GeneratorFunc p)
        {
            resolvers.Add(typeof(T), p);
        }

        public static T Resolve<T>()
        {
            if(!resolvers.ContainsKey(typeof(T)))
            {
                throw new UnknownTypeException();
            }

            var genFunc = resolvers[typeof(T)];
            var retVal = (T)genFunc();
            ResolveImports(retVal);
            return retVal;
        }

        public static void Reset()
        {
            resolvers.Clear();
        }

        private static object UntypedResolve(Type ty)
        {
            if (!resolvers.ContainsKey(ty))
            {
                throw new UnknownTypeException();
            }

            var genFunc = resolvers[ty];
            var retVal = genFunc();
            ResolveImports(retVal);
            return retVal;
        }

        public static void ResolveImports<T>(T target)
        {
            var ty = target.GetType();
            var fields = ty.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
                           .Where(x => x.CustomAttributes
                           .Any(y => y.AttributeType == typeof(MuImport)));

            foreach (var m in fields)
            {
                var bindingFlags = BindingFlags.SetField | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
                ty.InvokeMember(m.Name, bindingFlags , null, target, new object[] { UntypedResolve(m.FieldType) });
            }            
        }

    }
}
