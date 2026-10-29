namespace CinemaGo.Domain.Exceptions
{
    // Base ტიპი 
    public abstract class DomainException : Exception
    {
        protected DomainException(string message, Exception? inner = null) : base(message, inner) { }
    }

    // 404 - მოთხოვნილი რესურსი არ არსებობს
    public class NotFoundException : DomainException
    {
        public NotFoundException(string entity, object key)
            : base($"{entity} with id '{key}' was not found.") { }

        public NotFoundException(string message) : base(message) { }
    }

    // 409 - მოთხოვნა კონფლიქტს იწვევს, მაგ., ორმაგი ჯავშანი, გადაფარვა, უკვე გაუქმებულია...
    public class ConflictException : DomainException
    {
        public ConflictException(string message, Exception? inner = null) : base(message, inner) { }
    }

    // 403 - authenticated  არის მაგრამ დაშვებული არა - მაგ., სხვისი ჯავშნის გაუქმება
    public class ForbiddenException : DomainException
    {
        public ForbiddenException(string message) : base(message) { }
    }

    // 401 - არასწორი credential-ები 
    public class UnauthorizedException : DomainException
    {
        public UnauthorizedException(string message) : base(message) { }
    }

    // 422 - ბიზნეს წესის დარღვევა, მაგ., ადგილი სხვა დარბაზიდან, სესია რომელიც წარსულშია
    public class BusinessRuleException : DomainException
    {
        public BusinessRuleException(string message) : base(message) { }
    }
}
