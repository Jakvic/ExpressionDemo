using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks.Dataflow;

namespace ExpressionDemo
{
    public class RunData
    {
        private static Data data = new Data
        {
            Index = 1234214,
            Obj = new Pet
            {
                Cat = new Cat
                {
                    Color = "white",
                    Name = "pussy"
                }
            }
        };

        public static void test()
        {
            var filedName = "Obj.Cat.Name";
            var filter = new Filter();
            var flag = filter.FilterByFunc(data, filedName, (string z) => z == "pussy");
        }
    }
}