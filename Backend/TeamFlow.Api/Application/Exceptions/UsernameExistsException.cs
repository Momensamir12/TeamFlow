public class UsernameExistsException : Exception
{
    public UsernameExistsException() 
        : base($"username already exists") { }
}