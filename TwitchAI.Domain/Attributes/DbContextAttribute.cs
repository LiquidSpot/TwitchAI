namespace TwitchAI.Domain.Attributes
{

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class DbContextNameAttribute : Attribute
    {
        public string ContextName { get; }

        public DbContextNameAttribute(string contextName)
        {
            ContextName = contextName;
        }
    }
}
