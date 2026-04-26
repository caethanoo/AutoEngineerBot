# AutoEngineerBot 🤖

Bot desenvolvido em C# para monitorar preços de produtos no Mercado Livre. O sistema realiza web scraping avançado, extrai metadados e notifica o usuário via Telegram quando o preço atinge o alvo desejado.

## 🚀 Funcionalidades

- **Monitoramento em Tempo Real**: Extração de dados diretamente do Mercado Livre.
- **Integração com Telegram**: Alertas instantâneos com links de compra.
- **Banco de Dados Flexível**: Suporta MongoDB Local (Docker) ou MongoDB Atlas.
- **Seed Inteligente**: Cria automaticamente um produto de exemplo se o banco estiver vazio.

## 🛠️ Tecnologias

- **C# / .NET 8**
- **MongoDB**: Armazenamento de produtos e histórico de preços.
- **HtmlAgilityPack**: Parseamento de HTML para scraping.
- **Docker**: Facilidade para rodar o banco de dados localmente.
- **DotNetEnv**: Gerenciamento seguro de tokens e conexões.

## 📋 Como Instalar e Usar

### 1. Clonar o Repositório
```bash
git clone https://github.com/caethanoo/AutoEngineerBot.git
cd AutoEngineerBot
```

### 2. Configurar o Banco de Dados (Local via Docker)
A forma mais fácil de rodar é usando Docker:
```bash
docker run -d --name mongodb-local -p 27017:27017 mongo:latest
```

### 3. Configurar Variáveis de Ambiente
Crie um arquivo `.env` na pasta `AutoEngineerBot/` seguindo o modelo:

```env
TELEGRAM_BOT_TOKEN=seu_token_aqui
TELEGRAM_CHAT_ID=seu_chat_id_aqui
MONGO_CONNECTION=mongodb://localhost:27017/AutoEngineerDB
```

### 4. Executar o Projeto
```bash
cd AutoEngineerBot
dotnet run
```
