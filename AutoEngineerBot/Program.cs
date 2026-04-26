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
        
        string connectionString = Environment.GetEnvironmentVariable("MONGO_CONNECTION") ?? "";
        string botToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN") ?? "";
        string chatId = Environment.GetEnvironmentVariable("TELEGRAM_CHAT_ID") ?? "";
        
        var telegramService = new TelegramService(botToken, chatId);
        await telegramService.EnviarMensagemAsync("🤖 <b>AutoEngineerBot Ativado!</b>\nConectado ao MongoDB local e pronto para monitorar.");

        // Mostra apenas o início para conferir se está carregando o certo
        Console.WriteLine($"🔗 Usando conexão: {connectionString.Substring(0, Math.Min(connectionString.Length, 20))}... (Tamanho: {connectionString.Length})");


// 1. Criamos o cliente usando a connection string diretamente
var client = new MongoClient(connectionString);
var database = client.GetDatabase("AutoEngineerDB");
var colecaoProdutos = database.GetCollection<ProdutoMonitorado>("Produtos");

        Console.WriteLine("🔍 Consultando o MongoDB para saber os links de hoje...");

        try
        {
            var produtosParaMonitorar = await colecaoProdutos.Find(_ => true).ToListAsync();
            
            if (produtosParaMonitorar.Count == 0)
            {
                Console.WriteLine("✨ Banco de dados local vazio! Criando um produto de exemplo...");
                var exemplo = new ProdutoMonitorado 
                { 
                    Nome = "PlayStation 5", 
                    Url = "https://www.mercadolivre.com.br/console-playstation-5-slim-edico-digital-825-gb-branco-sony/p/MLB29001054?pdp_filters=item_id%3AMLB6010836248&from=gshop&matt_tool=91562990&matt_word=&matt_source=google&matt_campaign_id=22090193891&matt_ad_group_id=191545542882&matt_match_type=&matt_network=g&matt_device=c&matt_creative=787871501933&matt_keyword=&matt_ad_position=&matt_ad_type=pla&matt_merchant_id=735128188&matt_product_id=MLB29001054-product&matt_product_partition_id=2452780900542&matt_target_id=pla-2452780900542&cq_src=google_ads&cq_cmp=22090193891&cq_net=g&cq_plt=gp&cq_med=pla&gad_source=1&gad_campaignid=22090193891&gclid=Cj0KCQjw77bPBhC_ARIsAGAjjV9nw9bUktCG81_Nl-PkZzPuUW8yz0xkkH1ZfBGpK79-ij48w8bsdyEaAsT2EALw_wcB", 
                    PrecoAlvo = 2500m,
                    UltimoPreco = 0
                };
                await colecaoProdutos.InsertOneAsync(exemplo);
                produtosParaMonitorar.Add(exemplo);
                Console.WriteLine("✅ Produto de exemplo adicionado!\n");
            }
            else 
            {
                // Garante que o preço alvo seja o desejado (2500)
                var filtro = Builders<ProdutoMonitorado>.Filter.Eq(p => p.Nome, "PlayStation 5");
                var atualizacao = Builders<ProdutoMonitorado>.Update.Set(p => p.PrecoAlvo, 2500m);
                await colecaoProdutos.UpdateManyAsync(filtro, atualizacao);
                
                produtosParaMonitorar = await colecaoProdutos.Find(_ => true).ToListAsync();
            }

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
                        
                        string mensagem = $"🚨 <b>ALERTA DE PREÇO!</b> 🚨\n\n" +
                                         $"O produto <b>{produto.Nome}</b> atingiu o valor desejado!\n\n" +
                                         $"💰 Preço Atual: <b>R$ {produtoExtraido.PrecoAtual}</b>\n" +
                                         $"🎯 Seu Alvo: R$ {produto.PrecoAlvo}\n\n" +
                                         $"🛒 <a href=\"{produto.Url}\">CLIQUE AQUI PARA COMPRAR</a>";
                                         
                        await telegramService.EnviarMensagemAsync(mensagem);
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
        catch (MongoAuthenticationException ex)
        {
            Console.WriteLine($"\n❌ ERRO DE AUTENTICAÇÃO NO MONGODB:");
            Console.WriteLine($"O usuário ou a senha estão incorretos no seu arquivo .env.");
            Console.WriteLine($"Mensagem: {ex.Message}");
            Console.WriteLine($"\nO que conferir no MongoDB Atlas:");
            Console.WriteLine($"1. Se o usuário 'bcaethano21_db_user' existe em 'Database Access'.");
            Console.WriteLine($"2. Se a senha está correta (tente redefinir para uma simples no Atlas e atualizar o .env).");
            Console.WriteLine($"3. Se o IP 191.7.94.228 está liberado em 'Network Access'.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ DESCULPE, OCORREU UM ERRO INESPERADO: {ex.Message}");
            if (ex.InnerException != null) Console.WriteLine($"Detalhes: {ex.InnerException.Message}");
            Console.WriteLine();
        }
    }
}