using System;

namespace DifferentWay.Core
{
    public class GameManager
    {
        private static GameManager _instance;
        public static GameManager Instance => _instance ??= new GameManager();

        public Simulation Simulation { get; private set; }

        private GameManager()
        {
            Simulation = new Simulation();
        }

        public void StartMVP()
        {
            Simulation.InitializeMVP();
        }
    }
}
