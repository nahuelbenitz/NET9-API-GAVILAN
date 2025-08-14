﻿using Microsoft.EntityFrameworkCore;

namespace NET9.API.Utilidades
{
    public static class HttpContextExtensions 
    {
        public async static Task InsertarParametrosPaginacionEnCabecera<T>(this HttpContext httpContext, IQueryable<T> queryable)
        {
            if (httpContext is null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }
            double cantidad = await queryable.CountAsync();
            httpContext.Response.Headers.Append("cantidad-total-registros", cantidad.ToString());
        }
    }
}
