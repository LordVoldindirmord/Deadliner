namespace TelegramBot.State;

internal sealed class UserConversationStore
{
    private readonly Dictionary<long, string> _states = new();

    public bool TryGet(long chatId, out string? state)
        => _states.TryGetValue(chatId, out state);

    public void Set(long chatId, string state)
        => _states[chatId] = state;

    public void Remove(long chatId)
        => _states.Remove(chatId);
}
