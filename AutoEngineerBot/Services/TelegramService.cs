using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AutoEngineerBot.Services
{
    public class TelegramService
    {
        private readonly string _botToken;
        private readonly string _chatId;
        private readonly HttpClient _httpClient;

        public TelegramService(string botToken, string chatId)
        {
            _botToken = botToken ?? throw new ArgumentNullException(nameof(botToken));
            _chatId = chatId ?? throw new ArgumentNullException(nameof(chatId));
            _httpClient = new HttpClient();
        }

        public async Task EnviarMensagemAsync(string mensagem)
        {
            try 
            {
                string url = $"https://api.telegram.org/bot{_botToken}/sendMessage?chat_id={_chatId}&text={Uri.EscapeDataString(mensagem)}&parse_mode=HTML";
                Console.WriteLine($"📤 Tentando enviar mensagem para o Telegram (ChatID: {_chatId})...");
                var response = await _httpClient.GetAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("✅ Mensagem enviada com sucesso para o Telegram!");
                }
                else
                {
                    string errorBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"⚠️ Erro ao enviar para o Telegram: {response.StatusCode} - {errorBody}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Falha na comunicação com o Telegram: {ex.Message}");
            }
        }
    }
}
