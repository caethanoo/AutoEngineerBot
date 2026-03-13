using System.Threading.Tasks;

namespace AutoEngineerBot.Interfaces
{
    public interface INotificador
    {
        // O bot só conhece esse método. 
        // Se a mensagem vai pro Telegram, WhatsApp ou pombo correio, não importa para o núcleo do sistema.
        Task EnviarAlertaAsync(string mensagem);
    }
}