using Script.Objects;

namespace Script.Characters
{
    public class Luocha : Friendly
    {
        // 罗刹需要在失去自动战技冷却中的 Buff 时立刻检测当前生命值最低的角色的生命值是否低于 50%
        // 并且当多个角色生命值同时降至 50% 以下时, 其需要选择生命值最低的角色施放自动战技
        // 做起来还挺麻烦的, 而且还要给除了自己以外的队友上 不随回合减少的 Buff, 但是自己的 Buff 又是随回合减少的
    }
}