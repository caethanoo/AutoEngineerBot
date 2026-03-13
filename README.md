# AutoEngineerBot

Bot desenvolvido em C# para monitorar preços de produtos no Mercado Livre. Ele realiza web scraping na página do produto, extrai os dados via JSON-LD e notifica o usuário via Telegram quando o preço atinge o valor desejado.

## Tecnologias

- **C# / .NET 10**
- **HtmlAgilityPack**: Parseamento e análise do HTML.
- **System.Text.Json**: Leitura dos metadados extraídos do HTML.
- **Spectre.Console**: Interface no terminal.
- **DotNetEnv**: Gerenciamento de variáveis de ambiente (tokens).

## Como Instalar e Usar

1. Clone o repositório:
   ```bash
   git clone https://github.com/caethanoo/AutoEngineerBot.git
   cd AutoEngineerBot
   ```

### 2️⃣ Configure seu Telegram (Protegido por .env)
Nunca suba suas senhas para o GitHub! Nós utilizamos variáveis de ambiente para a sua segurança.
1. Fale com o `@BotFather` no Telegram e crie um novo Bot. Ele te dará um **Token**.
2. Pegue o seu próprio **Chat ID** (você pode descobrir mandando uma mensagem para o `@userinfobot` no Telegram).
3. Na raiz do projeto, crie um arquivo chamado **apenas** `.env` copiando o modelo `.env.example`.
4. Preencha o arquivo `.env` com os seus dados reais:

   ```env
   TELEGRAM_BOT_TOKEN=seu_token_aqui
   TELEGRAM_CHAT_ID=seu_chat_id_aqui
   ```

3. No arquivo `Program.cs`, ajuste a variável `urlAlvo` para o produto que deseja monitorar e o `precoDesejado` para a sua meta de preço.

4. Execute o projeto:
   ```bash
   cd AutoEngineerBot
   dotnet run
   ```
