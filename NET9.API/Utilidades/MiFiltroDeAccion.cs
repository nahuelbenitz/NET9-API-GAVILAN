using Microsoft.AspNetCore.Mvc.Filters;

namespace NET9.API.Utilidades
{
    public class MiFiltroDeAccion : IActionFilter
    {
        private readonly ILogger<MiFiltroDeAccion> _logger;

        public MiFiltroDeAccion(ILogger<MiFiltroDeAccion> logger)
        {
            _logger = logger;
        }

        //Se ejecuta antes de que se ejecute la acción del controlador
        public void OnActionExecuting(ActionExecutingContext context)
        {
            throw new NotImplementedException();
        }

        //se ejecuta después de que se ha ejecutado la acción del controlador
        public void OnActionExecuted(ActionExecutedContext context)
        {
            throw new NotImplementedException();
        }

       
    }
}
