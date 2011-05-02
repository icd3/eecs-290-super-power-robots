﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Project290.Games.SuperPowerRobots.Entities;
using Microsoft.Xna.Framework;

namespace Project290.Games.SuperPowerRobots.Controls
{
    class ModeAI:SPRAI
    {
        enum Mode
        {
            DEFENSE,
            RANGED,
            MELEE
        }

        enum Action
        {
            FLEE,
            CHARGE,
            DODGE
        }

        private Mode m_Mode;
        private Bot m_Self;
        private Bot m_Player;

        public ModeAI(SPRWorld world)
            : base(world)
        {
            foreach(Entity e in world.GetEntities())
            {
                if(e is Bot && ((Bot)e).IsPlayer())
                    m_Player = (Bot)e;
            }
        }

        public override void Update(float dTime, Bot self)
        {
            m_Self = self;
            this.chooseMode();
            Vector2 move = this.chooseMove();
            int side = chooseSide();

            float relRot = side * (float)Math.PI / 2f;
            float ownRot = self.GetRotation();

            Vector2 facing = new Vector2((float)Math.Cos(ownRot + relRot), (float)Math.Sin(ownRot + relRot));
            Vector2 desired = m_Player.GetPosition() - self.GetPosition();

            this.Spin = Math.Min(Math.Max(SPRWorld.SignedAngle(facing, desired) * 4, -1), 1);
            this.Move = move;
        }

        private void chooseMode()
        {
            m_Mode = Mode.DEFENSE;
        }

        private Vector2 chooseMove()
        {
            if (m_Mode == Mode.DEFENSE)
            {
                Vector2 toP = m_Player.GetPosition() - m_Self.GetPosition();
                Vector2[] corners = {new Vector2(300, 300) * Settings.MetersPerPixel, new Vector2(1620, 300) * Settings.MetersPerPixel, new Vector2(1620, 780) * Settings.MetersPerPixel, new Vector2(300, 780) * Settings.MetersPerPixel};
                List<Vector2> cornList = new List<Vector2>(corners);

                int bad = 0;
                for (int i = 0; i < cornList.Count; i++ )
                {
                    Vector2 pToCorn = cornList.ElementAt(i) - m_Player.GetPosition();
                    Vector2 badCorn = cornList.ElementAt(bad) - m_Player.GetPosition();
                    if (pToCorn.Length() < badCorn.Length()) bad = i;
                }

                cornList.RemoveAt(bad);

                Vector2[] toCorners = new Vector2[cornList.Count];

                int best = 0;
                for (int i = 0; i < toCorners.Length; i++)
                {
                    toCorners[i] = cornList[i] - m_Self.GetPosition();
                        if (Vector2.Dot(toCorners[i], toP) < Vector2.Dot(toCorners[best], toP))
                        {
                            best = i;
                        }
                }

                Vector2 move = toCorners[best];
                move.Normalize();
                return move;

                /*
                Vector2 toMid = new Vector2(960 * Settings.MetersPerPixel, 540 * Settings.MetersPerPixel) - m_Self.GetPosition();
                Vector2 pMid = new Vector2(960 * Settings.MetersPerPixel, 540 * Settings.MetersPerPixel) - m_Player.GetPosition();

                if (move.Length() > 300 * Settings.MetersPerPixel || Vector2.Dot(-move, toMid) <= 0)
                {
                    move.Normalize();
                    return move;
                }
                else if (pMid.Length() < 200 * Settings.MetersPerPixel)
                {
                    toMid.Normalize();
                    return -toMid;
                } else
                {
                    move.Normalize();
                    Vector2 sideStep = new Vector2(-move.Y, move.X);
                    return sideStep * Math.Sign(SPRWorld.SignedAngle(move, toMid));
                }*/
            }
            else
            {
                return Vector2.Zero;
            }
        }

        private bool[] chooseFire()
        {
            return new bool[4];
        }

        //choose the side of the bot to face towards the player
        private int chooseSide()
        {
            if (m_Mode == Mode.DEFENSE)
            {
                int bestShield = 0;
                Weapon[] weapons = m_Self.GetWeapons();
                for (int i = 0; i < 4; i++)
                {
                    if (weapons[i].GetWeaponType() == WeaponType.shield && (bestShield < 0 || weapons[bestShield].GetWeaponType() != WeaponType.shield || weapons[i].GetHealth() > weapons[bestShield].GetHealth()))
                        bestShield = i;
                }

                //if no shields, just go for weapon with greatest health
                if (bestShield < 0)
                {
                    bestShield = 0;
                    for (int i = 0; i < 4; i++)
                    {
                        if (weapons[i].GetHealth() > weapons[bestShield].GetHealth()) bestShield = i;
                    }
                }

                return bestShield;
            }
            else
            {
                return 0;
            }
        }
    }
}