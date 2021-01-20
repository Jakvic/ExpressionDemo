namespace ExpressionDemo
{
    public class Data
    {
        public int Index { get; set; }
        public object Obj { get; set; }
    }

    public class Store
    {
        public Good Good { get; set; }
    }

    public class Pet
    {
        public Cat Cat { get; set; }
    }

    public class Good
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Cat
    {
        public string Color { get; set; }
        public string Name { get; set; }
    }
}