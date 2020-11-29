using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using System;
using System.Linq;


namespace Assets
{
    public class PlayerBoard
    {
        public enum OrderCard {
            
            GetFood,
            ImproveReputation,
            DigTunnels,
            MineGold,
            RecruitImps,
            BuyTraps,
            HireMonster,
            BuildRoom,
            None
        };

        public enum DungeonTile
        {
            None,
            Tunnel,
            Room,
            Destroyed
        }

        enum Worker { Imp, Troll };

        private List<OrderCard> orderAccessible;
        private OrderCard[] orderInnaccessible;
        private OrderCard[] orderUsed;
        private bool[] availableMinion;


        private TrapCard trapStorage;
        private MonsterCard monsterLair;
        private AdventurerCard[] adventurer;
        private AdventurerCard paladin;
        private List<AdventurerCard> prison;
        private int coin;
        private int food;
        private List<Worker> workers;
        private int usedWorkers = 0;
        private int evilCounter;
        public DungeonTile[,] dungeon;
        private int taxMark;

        public int Coin { get => coin; set => coin = value; }
        public int Food { get => food; set => food = value; }
        public int EvilCounter { get => evilCounter; set => evilCounter = value; }
        public int TaxMark { get => taxMark; set => taxMark = value; }
        public int WorkersCount => workers.Count;

        public int WorkersUsed => usedWorkers;

        public List<OrderCard> OrderAccessible { get => orderAccessible; set => orderAccessible = value; }  //TODO: eventuellement, low priority, mettre fixe.
        public OrderCard[] OrderInnaccessible { get => orderInnaccessible; set => orderInnaccessible = value; }
        public OrderCard[] OrderUsed { get => orderUsed; set => orderUsed = value; }
        public bool[] AvailableMinion { get => availableMinion; set => availableMinion = value; }


        //CONSTRUCTOR
        public PlayerBoard()
        {
            dungeon = new DungeonTile[4, 5];
            adventurer = new AdventurerCard[3];

            coin = 3;
            food = 3;
            workers = new List<Worker>(){Worker.Imp, Worker.Imp, Worker.Imp};
            usedWorkers = 0;
            taxMark = 0;
            evilCounter = 4;

            availableMinion = new bool[3];
            for (int i = 0; i < availableMinion.Length; i++)
            {
                availableMinion[i] = true;
            }

            setStartingHand();
            setInitialTunnel();
            //... 
        }

        private void setStartingHand()
        {
            OrderAccessible = new List<OrderCard>();
            foreach (OrderCard oc in Enum.GetValues(typeof(OrderCard)))
            {
                if (oc == OrderCard.None) continue;

                OrderAccessible.Add(oc);
            }

            OrderInnaccessible = new OrderCard[2];
            setInitialNAOrderCard();

            OrderUsed = new OrderCard[3] { OrderCard.None, OrderCard.None, OrderCard.None };
        }

        private void setInitialNAOrderCard()
        {
            System.Random rndm = new System.Random();
            PlayerBoard.OrderCard oc;
            int index;

            for (int i = 0; i < 2; i++)
            {
                index = rndm.Next(OrderAccessible.Count);
                oc = OrderAccessible[index];
                OrderAccessible.RemoveAt(index);

                OrderInnaccessible[i]= oc;
            }
        }

        private void setInitialTunnel()
        {
            for (var x = 0; x < 4; ++x)
            {
                for (var y = 0; y < 5; ++y)
                {
                    dungeon[x, y] = DungeonTile.None;
                }
            }

            dungeon[0, 2] = DungeonTile.Tunnel;
            dungeon[1, 2] = DungeonTile.Tunnel;
            dungeon[2, 2] = DungeonTile.Tunnel;
        }

        public void DoCardRotation()
        {
            //TODO add condition if a minion wasnt used.            

            OrderAccessible.Add(OrderUsed[0]);
            OrderAccessible.Add(OrderInnaccessible[0]);
            OrderAccessible.Add(OrderInnaccessible[1]);

            OrderInnaccessible[0] = OrderUsed[1];
            OrderInnaccessible[1] = OrderUsed[2];

            OrderUsed[0] = PlayerBoard.OrderCard.None;
            OrderUsed[1] = PlayerBoard.OrderCard.None;
            OrderUsed[2] = PlayerBoard.OrderCard.None;
        }

        //Methods
        public void GainFood(int amount)
        {
            Food += amount;
        }

        public bool LoseFood(int amount)
        {
            if (Food <= 0) return false;
            
            Food -= amount;
            return true;
        }

        public void GainCoin(int amount)
        {
            Coin += amount;
        }

        public bool LoseCoin(int amount)
        {
            if (Coin <= 0) return false;
            
            Coin -= amount;
            return true;
        }

        //You can always pay evil but can't go over 15
        public bool RiseEvil(int amount)
        {
            if(EvilCounter < 15) EvilCounter += amount;
            return true;
        }

        public void LowerEvil(int amount)
        {
            if(EvilCounter <= 0) return;
            EvilCounter -= amount;
        }

        public void AddTaxMark(int amount)
        {
            TaxMark += amount;
        }

        public void GainImp()
        {
            workers.Add(Worker.Imp);
        }

        //Warning, I really mean lose not use
        public void LoseImp()
        {
            try
            {
                workers.Remove(workers.First(x => x == Worker.Imp));
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine(e);
            }
        }

        public bool UseImp()
        {
            if (usedWorkers < WorkersCount)
            {
                usedWorkers++;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void SubstractTaxMark(int amount)
        {
            TaxMark -= amount;
        }

        


        public bool ValidateTilePlacement(Point position)
        {
            if (ValidateTunnelPlacement(position))
            {
                dungeon[position.X, position.Y] = DungeonTile.Tunnel;
                return true;
            }

            return false;
        }
        //validate that the tunnel is place on an empty place and that it is the continuation of something. Also validate that it does not create a 2x2 square on the board
        private bool ValidateTunnelPlacement(Point position)
        {
            if (dungeon[position.X, position.Y] == DungeonTile.None)
            {
                if (VerifyIfTileNotNone(new Point(position.X - 1, position.Y))     ||
                    VerifyIfTileNotNone(new Point(position.X + 1, position.Y))     ||
                    VerifyIfTileNotNone(new Point(position.X, position.Y - 1))     ||
                    VerifyIfTileNotNone(new Point(position.X, position.Y + 1)))
                {
                    if (DoesNotCreateSquare(position))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool VerifyIfTileNotNone(Point position)
        {
            //Verify if out of bound
            if (position.X < 0 || position.X  > 3 || position.Y < 0 || position.Y  > 4)
                return false;
            return dungeon[position.X , position.Y] != DungeonTile.None;
        }

        private bool DoesNotCreateSquare(Point position)
        {
            //if not on the left or top edge and would make a square
            if ((position.X > 0 || position.Y > 0) &&
               (VerifyIfTileNotNone(new Point(position.X - 1, position.Y)) &&
                VerifyIfTileNotNone(new Point(position.X, position.Y - 1)) &&
                VerifyIfTileNotNone(new Point(position.X - 1, position.Y - 1))))
            {
                return false;
            }

            //if not on the right or top edge and would make a square
            if ((position.X < 3 || position.Y > 0) &&
               (VerifyIfTileNotNone(new Point(position.X + 1, position.Y)) &&
                VerifyIfTileNotNone(new Point(position.X, position.Y - 1)) &&
                VerifyIfTileNotNone(new Point(position.X + 1, position.Y - 1))))
            {
                return false;
            }

            //if not on the right or bottom edge and would make a square
            if ((position.X < 3 || position.Y < 4) &&
               (VerifyIfTileNotNone(new Point(position.X + 1, position.Y)) &&
                VerifyIfTileNotNone(new Point(position.X, position.Y + 1)) &&
                VerifyIfTileNotNone(new Point(position.X + 1, position.Y + 1))))
            {
                return false;
            }

            //if not on the left or bottom edge and would make a square
            if ((position.X > 0 || position.Y < 4) &&
               (VerifyIfTileNotNone(new Point(position.X - 1, position.Y)) &&
                VerifyIfTileNotNone(new Point(position.X, position.Y + 1)) &&
                VerifyIfTileNotNone(new Point(position.X - 1, position.Y + 1))))
            {
                return false;
            }

            return true;
        }

        bool PlaceRoom(DungeonTile room, Point position)
        {
            if (ValidateRoomPlacement(position))
            {
                dungeon[position.X, position.Y] = room;
                return true;
            }

            return false;

        }

        //validate that the place where the room is going was a tunnel and that there is at least one tunnel and no room beside it 
        private bool ValidateRoomPlacement(Point position)
        {
            if (dungeon[position.X, position.Y] == DungeonTile.Tunnel)
            {
                if (dungeon[position.X - 1, position.Y] != DungeonTile.Room &&
                    dungeon[position.X + 1, position.Y] != DungeonTile.Room &&
                    dungeon[position.X, position.Y - 1] != DungeonTile.Room &&
                    dungeon[position.X, position.Y + 1] != DungeonTile.Room)
                {
                    if (dungeon[position.X - 1, position.Y] == DungeonTile.Tunnel ||
                        dungeon[position.X + 1, position.Y] == DungeonTile.Tunnel ||
                        dungeon[position.X, position.Y - 1] == DungeonTile.Tunnel ||
                        dungeon[position.X, position.Y + 1] == DungeonTile.Tunnel)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        void AddPrisonner(AdventurerCard adventurer)
        {
            prison.Add(adventurer);
        }


        public void ResetUsedImps()
        {
            usedWorkers = 0;
        }
    }
}