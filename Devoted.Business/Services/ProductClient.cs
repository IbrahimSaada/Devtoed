using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Devoted.Business.Error;
using Devoted.Business.Interfaces;
using Devoted.Domain.Sql.Dto;
using Devoted.Domain.Sql.Response.Base;
using Microsoft.Extensions.Logging;

namespace Devoted.Business.Services
{
    public class ProductClient : IProductClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<ProductClient> _log;

        public ProductClient(HttpClient http, ILogger<ProductClient> log)
        {
            _http = http;
            _log = log;
        }

        public async Task<ProductDto> GetByIdAsync(long id, CancellationToken ct)
        {
            using var resp = await _http.GetAsync($"get/{id}", ct);
            if (resp.StatusCode == HttpStatusCode.NotFound)
                throw new ItemNotFoundOrNullError($"Product {id} not found");

            resp.EnsureSuccessStatusCode();
            var br = await resp.Content.ReadFromJsonAsync<BaseResponse>(cancellationToken: ct);
            return JsonSerializer.Deserialize<ProductDto>(
                JsonSerializer.Serialize(br!.Data),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            )!;
        }
    }
}
