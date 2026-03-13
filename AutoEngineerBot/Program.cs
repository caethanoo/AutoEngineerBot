using System;
using System.Threading.Tasks;
using Spectre.Console;
using AutoEngineerBot.Services;
using DotNetEnv;

Env.TraversePath().Load();

Console.Clear();
AnsiConsole.Write(
    new FigletText("AutoEngineer Bot")
        .Centered()
        .Color(Color.Blue));

var extrator = new ExtratorMercadoLivre();
string urlAlvo = "https://www.mercadolivre.com.br/console-sony-playstation-5-edico-slim-disk-1tb-branco-controle-sem-fio-dualsense-ps5-branco/p/MLB52897777";

decimal precoDesejado = 10000.00m; 

while (true)
{
    AnsiConsole.MarkupLine($"\n[bold yellow]Iniciando varredura... Alvo: Menor que R$ {precoDesejado}[/]");

    try 
    {
        var produto = await AnsiConsole.Status()
            .StartAsync("Acessando a rede e quebrando a criptografia...", async ctx => 
            {
                return await extrator.ExtrairAsync(urlAlvo);
            });

        var tabela = new Table();
        tabela.Border(TableBorder.Rounded);
        tabela.AddColumn("[green]Produto[/]");
        tabela.AddColumn(new TableColumn("[green]Preço Encontrado[/]").Centered());
        tabela.AddColumn(new TableColumn("[green]Data da Coleta[/]").Centered());

        string corPreco = produto.PrecoAtual <= precoDesejado ? "green" : "red";

        tabela.AddRow(
            $"[bold white]{produto.Nome}[/]", 
            $"[bold {corPreco}]R$ {produto.PrecoAtual}[/]", 
            $"[grey]{produto.DataConsulta}[/]"
        );

        AnsiConsole.Write(tabela);

        if (produto.PrecoAtual <= precoDesejado)
        {
            AnsiConsole.MarkupLine("\n[bold green blink]🎉 PREÇO ALVO ATINGIDO! HORA DE COMPRAR! 🎉[/]");
            
            string tokenTelegram = Env.GetString("TELEGRAM_BOT_TOKEN");
            string chatId = Env.GetString("TELEGRAM_CHAT_ID");
            
            if(string.IsNullOrEmpty(tokenTelegram) || string.IsNullOrEmpty(chatId))
            {
                AnsiConsole.MarkupLine("[bold red]⚠️ ERRO: Configure TELEGRAM_BOT_TOKEN e TELEGRAM_CHAT_ID no seu arquivo .env![/]");
            }
            else
            {
                string mensagem = $"🚨 ALERTA DO SEU BOT! 🚨\nO PS5 caiu para R$ {produto.PrecoAtual}!\nCorre para o link:\n{urlAlvo}";

                string urlTelegram = $"https://api.telegram.org/bot{tokenTelegram}/sendMessage?chat_id={chatId}&text={Uri.EscapeDataString(mensagem)}";

                using (HttpClient client = new HttpClient())
                {
                    await client.GetAsync(urlTelegram);
                    AnsiConsole.MarkupLine("[bold blue]📱 Bip Bip! Mensagem enviada para o seu celular![/]");
                }
            }
        }
    }
    catch (Exception ex)
    {
        AnsiConsole.MarkupLine($"\n[bold red]Erro na varredura:[/] {ex.Message}");
    }

    AnsiConsole.MarkupLine("[grey]O bot vai dormir por 10 segundos...\n-----------------------------------\n[/]");
    await Task.Delay(TimeSpan.FromSeconds(10)); 
}