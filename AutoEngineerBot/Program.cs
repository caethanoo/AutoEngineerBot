using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using DotNetEnv;
using AutoEngineerBot.Models;
using AutoEngineerBot.Services;

class Program
{
    static async Task Main(string[] args)
    {
        Console.Clear();
        Console.WriteLine("Iniciando teste de conexão com o MongoDB...");

        Env.TraversePath().Load();
        string connectionString = Environment.GetEnvironmentVariable("MONGO_CONNECTION");

        var client = new MongoClient(connectionString);
        var database = client.GetDatabase("AutoEngineerDB");
        var colecaoProdutos = database.GetCollection<ProdutoMonitorado>("Produtos");

        Console.WriteLine("🔍 Consultando o MongoDB para saber os links de hoje...");

       
        var produtosParaMonitorar = await colecaoProdutos.Find(_ => true).ToListAsync();
        Console.WriteLine($"Encontrei {produtosParaMonitorar.Count} produtos para olhar hoje!\n");

        var extrator = new ExtratorMercadoLivre();
        
        foreach (var produto in produtosParaMonitorar)
        {
            Console.WriteLine($"- Produto: {produto.Nome}");
            Console.WriteLine($"  Preço Alvo: R$ {produto.PrecoAlvo}");
            Console.WriteLine($"  LINK >> {produto.Url}");
            
            try 
            {
                var produtoExtraido = await extrator.ExtrairAsync(produto.Url);
                
                Console.WriteLine($"  Preço Atual no ML: R$ {produtoExtraido.PrecoAtual}");
                
                if (produtoExtraido.PrecoAtual <= produto.PrecoAlvo)
                {
                    Console.WriteLine("  🚨🚨🚨 ALERTA: O PREÇO CAIU PARA O SEU ALVO! HORA DE COMPRAR! 🚨🚨🚨");
                }
                else 
                {
                    Console.WriteLine("  ❌ O preço ainda está alto. Vamos continuar monitorando.");
                }

                // Atualizar o preço no banco de dados para o próximo dia
                var filtro = Builders<ProdutoMonitorado>.Filter.Eq(p => p.Id, produto.Id);
                var atualizacao = Builders<ProdutoMonitorado>.Update.Set(p => p.UltimoPreco, produtoExtraido.PrecoAtual);
                
                await colecaoProdutos.UpdateOneAsync(filtro, atualizacao);
                Console.WriteLine("  💾 Preço atualizado no banco de dados com sucesso!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ⚠️ Erro ao tentar ler o Mercado Livre: {ex.Message}");
            }
            
            Console.WriteLine("------------------------------------------");
        }
    }
}