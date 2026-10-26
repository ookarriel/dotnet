namespace fifo.Api.Services;

public class NotFoundException(string message) : Exception(message);
public class ConflictException(string message) : Exception(message);
public class BusinessRuleException(string message) : Exception(message);
public class InvalidCredentialsException() : Exception("Credenciales inválidas o usuario inactivo.");
