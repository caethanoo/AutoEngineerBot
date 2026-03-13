using System;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using AutoEngineerBot.Models;
using AutoEngineerBot.Interfaces;
using System.Text.Json;

namespace AutoEngineerBot.Services
{
    public class ExtratorMercadoLivre : IExtratorDePrecos
    {
        private static readonly HttpClient client = new HttpClient();

        public async Task<Produto> ExtrairAsync(string url)
        {
            string htmlBruto = await client.GetStringAsync(url);

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(htmlBruto);

            var nodoCofreJson = doc.DocumentNode.SelectSingleNode("//script[@type='application/ld+json' and contains(text(), 'offers')]");

            if (nodoCofreJson == null)
            {
                throw new Exception("Não achei a tag de PREÇOS (offers) na página! A planta da casa mudou.");
            }

            string textoJson = nodoCofreJson.InnerText;

            using JsonDocument docJson = JsonDocument.Parse(textoJson);
            JsonElement root = docJson.RootElement;
            
            if (!root.TryGetProperty("name", out JsonElement outName) || 
                !root.TryGetProperty("offers", out JsonElement outOffers))
            {
                throw new Exception("Propriedades de nome ou preços faltando no JSON!");
            }

            string nomeProduto = outName.GetString() ?? "Nome não encontrado";
            
            JsonElement priceElement;
            if (outOffers.ValueKind == JsonValueKind.Array && outOffers.GetArrayLength() > 0)
            {
                priceElement = outOffers[0].GetProperty("price");
            }
            else
            {
                priceElement = outOffers.GetProperty("price");
            }

            decimal precoProduto = 0m;
            if (priceElement.ValueKind == JsonValueKind.Number)
            {
                precoProduto = priceElement.GetDecimal();
            }
            else if (priceElement.ValueKind == JsonValueKind.String)
            {
                decimal.TryParse(priceElement.GetString(), System.Globalization.CultureInfo.InvariantCulture, out precoProduto);
            }

            Console.WriteLine("\n--- HACKEAMOS O COFRE! VEJA OS DADOS EXTRAÍDOS ---");
            Console.WriteLine($"Nome: {nomeProduto}");
            Console.WriteLine($"Preço: {precoProduto}");

            return new Produto(nomeProduto, precoProduto);
        }
    }
}