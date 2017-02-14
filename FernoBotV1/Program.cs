namespace FernoBotV1
{
    class Program
    {
        static void Main(string[] args) => new FernoBot().RunAndBlockAsync(args).GetAwaiter().GetResult();
    }
}
