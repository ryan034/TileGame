using System;
using static GlobalData;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    //holds economy, piety choices
    /*you unlock a new hero at every upgrade. the heroes xp starts at the average xp of all your other heroes. every age should give your passive upgrades to your units of lower age.
     * some ages will give you specific upgrades to units of the same age that is rare.ages selection works like choosing a god in age of mythology.
     * every race will have 3 ages. every age will have 3 options 
     * age will be will randomly generated giving you 2 of 3 choices at every age, 3 paths are generated
     * eg. first path will give you hero 1 - choice 1, (1,2) and (2,3) 
     * second path will give you hero 2 - choice 2, (2,3) and (2,3)
     * third path will give you hero 3 -choice 3, (1,3) and (1,3)
     * how paths are generated: for every choice at the first age: 
     * pick a random hero out that is not picked (4 in total)
     * pick a random combination of 2 choices at age 2 not picked and a random hero not spawned at age 1 (the hero choice is randomly generated after hero choice at age 1)
     * pick a random combination of 2 choices at age 3 not picked and a random hero not spawned at age 1 or 2 (the hero choice is randomly generated after hero choice at age 1 and 2)
     *
    */
    public static PlayerManager globalInstance;

    private Dictionary<int, Player> players = new Dictionary<int, Player>();
    private List<int> turnOrder = new List<int>();
    private int turnCount = -1; //first turn is 0
    private int TeamsTotal => players.Count;
    private int RoundCount => turnCount % TeamsTotal; // number of completed rounds

    public int TeamTurn => turnOrder[turnCount % TeamsTotal]; //team thats taking current turn
    public int ClockFrame => RoundCount % clockTotal; // time of day night cycle, 0 is dawn/morning, 1 is midday, 2 is afternoon, 3 dusk/evening, 4 is night, is 5 late night approaching early dawn
    public bool IsDay => ClockFrame / (clockTotal * 1.0) < 0.5 ? true : false;

    private class Player
    {
        public int wood;
        public int gold;
        public string race;
    }

    /*
    private Dictionary<int, PietyNode> pietyTrees;

    private class PietyNode
    {
        public List<PietyNode> forwardNodes = new List<PietyNode>();
        public readonly string name;
        public readonly string description;
        public int hero; //0-3 theres only 4 heroes
        public bool chosen;

    }
    */

    private void Awake()
    {
        if (globalInstance == null)
        {
            globalInstance = this;
        }
        else if (globalInstance != this)
        {
            Destroy(gameObject);
        }
    }

    /*
    private PietyNode GenerateTree(UnitBase.Race race)
    {
        //load race
        PietyNode root = new PietyNode();
        List<int> heroes = new List<int> { 0, 1, 2, 3 };
        foreach (PietyNode node in )
        {
            node.hero = Random.Range(1, heroes.Count + 1);//pick random out of list
            root.forwardNodes.Add(node);


            node.forwardNodes.Add()
        }

        return root;
    }*/
    /*
    public void LoadPlayers(int teams)
    {
        for (int i = 0; i < teams; i++)
        {
            players[i] = new Player();
        }
    }*/

    public void LoadPlayer(int team)
    {
        if (!players.ContainsKey(team)) { players[team] = new Player(); }
        if (!turnOrder.Contains(team)) { turnOrder.Add(team); }
    }

    public void EndAndStartNextTurn()
    {
        turnCount = turnCount + 1;
        TileManager.globalInstance.EndAndStartNextTurn(TeamTurn);
    }
}