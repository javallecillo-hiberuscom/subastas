using System;
using System.Collections.Generic;
using Subastas.Application.Dtos;

namespace Subastas.Application.Interfaces.Services
{
    public interface IPdfService
    {
        byte[] GenerateSubastasPdf(IEnumerable<SubastaDto> items, DateTime from, DateTime to);
    }
}