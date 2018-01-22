using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Team
{
    public interface ITeamController
    {
        int GetId();

        void AddPlayer(int playerId);

        bool HasPlayer(int playerId);

        int GetCoin();

        int SpendGold(int amount);


    }
}
