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
        Console.WriteLine("Iniciando teste de conexão com o MongoDB...");

        Env.TraversePath().Load();
        
        string connectionString = Environment.GetEnvironmentVariable("MONGO_CONNECTION");

// 1. Lemos as configurações originais do link
var configuracoesMongo = MongoClientSettings.FromConnectionString(connectionString);

// 2. Criamos o Bypass de Segurança para o Windows não interferir
var sslSettings = new SslSettings
{
    ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true
};
configuracoesMongo.SslSettings = sslSettings;

// 3. Criamos o cliente usando essas novas configurações
var client = new MongoClient(configuracoesMongo);
        var database = client.GetDatabase("AutoEngineerDB");
        var colecaoProdutos = database.GetCollection<ProdutoMonitorado>("Produtos");

        Console.WriteLine("🔍 Consultando o MongoDB para saber os links de hoje...");

        try
        {
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
        catch (TimeoutException ex)
        {
            Console.WriteLine($"\n❌ ERRO FATAL DE CONEXÃO COM O MONGODB:");
            Console.WriteLine($"O Windows impediu a conexão segura com o banco de dados. Motivo: {ex.Message}");
            Console.WriteLine($"\nDica: Verifique seu antivírus, firewall ou se a data/hora do seu relógio do Windows está correta.\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ DESCULPE, OCORREU UM ERRO INESPERADO: {ex.Message}\n");
        }
    }
}