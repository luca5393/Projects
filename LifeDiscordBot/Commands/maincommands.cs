using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace LifeDiscordBot.Commands
{

    public class maincommands : ApplicationCommandModule
    {
        public DatabaseManager db = new();

        [SlashCommand("startlife", "Start a new life")]
        public async Task startlifecommand(InteractionContext ctx)
        {

            var embed = new DiscordEmbedBuilder();



            if (db.Userexists(ctx.User.Id))
            {
                embed.Title = "User already exists";
                embed.Color = DiscordColor.Red;
            }
            else
            {
                db.CreateUserProfile(ctx.User.Id, ctx.User.Username, ctx.User.AvatarUrl);
                embed.Title = "Life is now started";
                embed.Description = "/work apply company(LifeBotCompany) to get a job";
                embed.Color = DiscordColor.Green;
            }

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));

        }

        [SlashCommand("profile", "Look at your profile")]
        public async Task profilecommand(InteractionContext ctx, [Option("User", "The User You want to show the profile of")] DiscordUser? giveuser = null)
        {
            try
            {
                if (giveuser == null)
                {
                    giveuser = ctx.User;
                }
                var embed = new DiscordEmbedBuilder();
                if (db.Userexists(giveuser.Id))
                {
                    var user = db.Userget(giveuser.Id);

                    embed.Title = $"{user.username}'s Account";
                    embed.Color = DiscordColor.Azure;
                    embed.WithThumbnail(user.profileurl);
                    if (!string.IsNullOrEmpty(user.workplace))
                    {
                        embed.AddField("Job", $"{user.workplace} - Cut {user.cut}");
                    }
                    else
                    {
                        embed.AddField("Job", $"None");
                    }
                    embed.AddField("Current Location", $"{user.location}");
                    embed.AddField("Money", $"${user.money}");
                    int i = 0;
                    var allcompanylist = db.companygetofuser(user.id);
                    string comps = "";
                    foreach (var comp in allcompanylist)
                    {
                        i++;
                        if (i == allcompanylist.Count)
                        {
                            comps += comp.name;
                        }
                        else
                        {
                            comps += comp.name + ", ";
                        }
                    }
                    if (comps == "")
                    {
                        embed.AddField("Companies", "None");
                    }
                    else
                    {
                        embed.AddField("Companies", comps);
                    }



                }
                else
                {
                    embed.Title = "User doesn't exist, type /startlife to create a user.";
                    embed.Color = DiscordColor.Red;
                }
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }

        [SlashCommand("companystats", "Look at a companies profile")]
        public async Task companystatscommand(InteractionContext ctx, [Option("Company", "The Company You want to show the profile of")] string giveuser)
        {
            try
            {

                var embed = new DiscordEmbedBuilder();
                if (db.companyexists(giveuser))
                {
                    var company = db.companyget(giveuser);

                    var owner = db.Userget(company.owner);

                    embed.Title = $"{company.name}";
                    embed.Color = DiscordColor.Azure;
                    embed.AddField("Owner", $"{owner.username}");
                    embed.AddField("Type", $"{company.type}");

                }
                else
                {
                    embed.Title = "Company dont exist. /startup to make a company";
                    embed.Color = DiscordColor.Red;
                }
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }

        [SlashCommand("company", "Manage your company.")]
        public async Task companycommand(InteractionContext ctx,
                                                                [
                                                                Choice("Fire", "fire"),
                                                                Choice("Hire", "hire"),
                                                                Choice("Deny", "deny"),
                                                                Choice("Promote", "promote"),
                                                                Choice("Demote", "demote"),
                                                                ]
                                                                [Option("Action","The action you want to take")] string action,
                                                                [Option("Company", "The company you want")] string companyinquestion,
                                                                [Option("User", "The worker you wish to modify")] DiscordUser giveuser)
        {

            var embed = new DiscordEmbedBuilder();
            if (db.Userexists(ctx.User.Id) && db.Userexists(giveuser.Id))
            {
                var owneruser = db.Userget(ctx.User.Id);
                if (db.companyexists(companyinquestion))
                {
                    var company = db.companyget(companyinquestion);
                    if (company.owner == owneruser.id)
                    {
                        var user = db.Userget(giveuser.Id);

                        switch (action)
                        {
                            case "fire":
                                embed.Title = $"{user.username} fired from {user.workplace}.";
                                user.workplace = null;
                                embed.Color = DiscordColor.Green;
                                break;
                            case "hire":

                                if (user.id == company.owner)
                                {
                                    user.workplace = company.name;
                                    user.cut = 100;
                                    embed.Title = $"{user.username} is now working at his company";
                                    db.applydelete(user.id, company.name);
                                    embed.Color = DiscordColor.Green;
                                }
                                else if (db.applyexists(user.id, company.name))
                                {
                                    user.workplace = company.name;
                                    user.cut = 30;
                                    embed.Title = $"{user.username} was hired at {user.workplace}.";
                                    embed.Description = $"The default cut is 30%";
                                    db.applydelete(user.id, company.name);
                                    embed.Color = DiscordColor.Green;
                                }
                                else
                                {
                                    embed.Title = $"Apply request in question does not exist.";
                                    embed.Color = DiscordColor.Red;
                                }
                                break;
                            case "deny":

                                if (db.applyexists(user.id, company.name))
                                {
                                    embed.Title = $"{user.username}'s application for working at {company.name} was denied.";
                                    db.applydelete(user.id, company.name);
                                    embed.Color = DiscordColor.Green;
                                }
                                else
                                {
                                    embed.Title = $"Apply request in question does not exist.";
                                    embed.Color = DiscordColor.Red;
                                }
                                break;
                            case "promote":
                                if (user.cut <= 95)
                                {
                                    user.cut = user.cut + 5;

                                    embed.Title = $"{user.username} income cut changed to {user.cut}.";
                                    embed.Color = DiscordColor.Green;
                                }
                                else
                                {
                                    embed.Title = $"Income cut modification unsuccessful, user cut is currently {user.cut}.";
                                    embed.Color = DiscordColor.Red;
                                }
                                break;
                            case "demote":
                                if (user.cut >= 5)
                                {
                                    user.cut = user.cut - 5;

                                    embed.Title = $"{user.username}'s income cut changed to {user.cut}.";
                                    embed.Color = DiscordColor.Green;
                                }
                                else
                                {
                                    embed.Title = $"Income cut modification unsuccessful, user cut is currently {user.cut}.";
                                    embed.Color = DiscordColor.Red;
                                }
                                break;
                        }
                        db.Userupdate(user);

                    }
                    else
                    {
                        embed.Title = "Error";
                        embed.Description = "You dont own that company.";
                        embed.Color = DiscordColor.Red;
                    }
                }
                else
                {

                    embed.Title = "Error";
                    embed.Description = "Company does not exist.";
                    embed.Color = DiscordColor.Red;

                }
            }
            else
            {
                embed.Title = "Error";
                embed.Description = "One or more users do not exist.";
                embed.Color = DiscordColor.Red;
            }

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));

        }


        [SlashCommand("work", "Work at a user's company")]
        public async Task workcommand(InteractionContext ctx,
                                                            [
                                                                Choice("Resign", "resign"),
                                                                Choice("Apply", "apply")
                                                            ]
                                                            [Option("action","The action you want to take")] string action,
                                                            [Option("company", "The company")] string? companyinquestion = null
                                                            )

        {
            var embed = new DiscordEmbedBuilder();

            if (db.Userexists(ctx.User.Id))
            {
                var user = db.Userget(ctx.User.Id);

                switch (action)
                {
                    case "resign":
                        if (db.companyexists(user.workplace))
                        {
                            var company = db.companyget(user.workplace);
                            embed.Title = $"{user.username} has resigned from {company.name}";

                            user.workplace = null;
                            db.Userupdate(user);
                        }
                        else
                        {
                            embed.Title = "Error";
                            embed.Description = "One or more users do not exist.";
                            embed.Color = DiscordColor.Red;
                        }
                        break;
                    case "apply":
                        if (db.companyexists(companyinquestion))
                        {
                            var company = db.companyget(companyinquestion);
                            if (company.owner == 1181137497896530081)
                            {
                                embed.Title = "Successfully sent application.";
                                embed.Description = $"{user.username} was hired at {companyinquestion}.";
                                embed.Color = DiscordColor.Green;

                                user.workplace = company.name;
                                user.cut = 30;
                                db.Userupdate(user);
                            }
                            else
                            {

                                if (db.applyexists(user.id, companyinquestion))
                                {
                                    embed.Title = "Error";
                                    embed.Description = $"You cant apply to the same company while another application is in progress.";
                                    embed.Color = DiscordColor.Red;
                                }
                                else
                                {
                                    embed.Title = "Successfully sent application.";
                                    embed.Description = $"{user.username} has applied at {companyinquestion}.";
                                    embed.Color = DiscordColor.Green;
                                    db.Createworkapply(user.id, companyinquestion);
                                }
                            }
                        }
                        else
                        {
                            embed.Title = "Error";
                            embed.Description = "Company dont exist";
                            embed.Color = DiscordColor.Red;
                        }
                        break;
                }


            }
            else
            {
                embed.Title = "Error";
                embed.Description = "You have to have a profile";
                embed.Color = DiscordColor.Red;
            }
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }

        [SlashCommand("goto", "Go to a location")]
        public async Task gotocommand(InteractionContext ctx,
                                                            [
                                                            Choice("Town Square", "town square"),
                                                                Choice("Work", "work")
                                                                ]
                                                                [Option("title", "dis")] string action)
        {
            var embed = new DiscordEmbedBuilder();

            if (db.Userexists(ctx.User.Id))
            {
                var user = db.Userget(ctx.User.Id);
                if (user.location == action)
                {
                    embed.Title = $"You're already at {user.location}";
                    embed.Color = DiscordColor.Red;
                }
                else if (user.location == "work")
                {
                    if (db.companyexists(user.workplace))
                    {
                        var company = db.companyget(user.workplace);
                        if (company.owner == user.id)
                        {
                            TimeSpan timethere = DateTime.Now - user.time;


                            //npc production
                            //4,19 with worth of 500, 1 hour, 1 NPC and 1x earnings
                            //41,9 with worth of 500, 1 hour, 10 NPC's and 1x earnings
                            double production = company.worth / 429496.7294 * Math.Round(timethere.TotalSeconds) * company.npcworkers * company.earnings;

                            //owner production
                            //41,9 with worth of 500, 1 hour and 1x earnings
                            production += company.worth / 42949.67294 * Math.Round(timethere.TotalSeconds) * company.earnings;

                            //operationcost
                            // 20.88 after 1 hour, 5 op cost and 1 factory
                            production -= Math.Round(timethere.TotalSeconds) * 0.00116 * company.operatingcost * company.factories;



                            user.money += (int)Math.Round(production);

                            embed.Title = $"Moving to {action}";
                            embed.Description = $"You have worked for {timethere.ToString(@"dd\:hh\:mm\:ss")} and earned ${Math.Round(production)}";
                            embed.Color = DiscordColor.Green;
                            user.location = action;
                            user.time = DateTime.Now;

                            db.Userupdate(user);
                        }
                        else
                        {
                            var owneruser = db.Userget(company.owner);

                            TimeSpan timethere = DateTime.Now - user.time;

                            double production = company.worth / 2 / 2147483.647 * 1000.0 * Math.Round(timethere.TotalSeconds) * company.earnings / 10;

                            double cut = production * user.cut / 100;

                            double ownerincome = (production * (100 - user.cut) / 100) - (Math.Round(timethere.TotalSeconds) * 0.1 * company.operatingcost / 10);

                            user.money += (int)Math.Round(cut);
                            owneruser.money += (int)Math.Round(ownerincome);

                            embed.Title = $"Moving to {action}";
                            embed.Description = $"You have worked for {timethere.ToString(@"dd\:hh\:mm\:ss")} and earned ${Math.Round(cut)} and {company.name} earned ${Math.Round(ownerincome)}";
                            embed.Color = DiscordColor.Green;
                            user.location = action;
                            user.time = DateTime.Now;

                            db.Userupdate(user);
                            db.Userupdate(owneruser);
                        }
                    }
                    else
                    {
                        embed.Title = "Company does not exist.";
                        embed.Color = DiscordColor.Red;
                    }
                }
                else
                {
                    if (action == "work" && !db.companyexists(user.workplace))
                    {
                        embed.Title = $"Cant move to work because you dont have a job";
                        embed.Color = DiscordColor.Red;
                    }
                    else
                    {
                        embed.Title = $"Moving to {action}";
                        embed.Color = DiscordColor.Green;
                        user.location = action;
                        user.time = DateTime.Now;
                        db.Userupdate(user);
                    }
                }
            }
            else
            {
                embed.Title = "User does not exist.";
                embed.Description = "/startlife to make a profile";
                embed.Color = DiscordColor.Red;
            }

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }

        [SlashCommand("startup", "Make a company")]
        public async Task startupcommand(InteractionContext ctx, [Option("Name", "The name of your company.")] string name,
                                                            [

            Choice("ITsupport($500)", "ITsupport"), // 1x earnings // $500
            Choice("Drugs(Illegal)($1000)", "Drugs"), // 2.5x earnings // $1000
            Choice("Farming($5.000)", "Farming"), // 2x earnings // $5.000
            Choice("Education($10.000)", "Education"), // 2.5x earnings // $10.000
            Choice("Restaurant($25.000)", "Restaurant"), // 3x earnings // $25.000
            Choice("Transport($75.000)", "Transport"), // 5x earnings // $75.000
            Choice("Furniture($100.000)", "Furniture"), // 7x earnings // $100.000
            Choice("Construction($150.000)", "Construction"), // 10x earnings // $150.000
            Choice("Scamcenter(Illegal)($350.000)", "Scamcenter"), // 14x earnings // $350.000
            Choice("Airline($750.000)", "Airline"), // 13x earnings // $ 750.000
            Choice("Electronics($2.000.000)", "Electronics Manufacturing"), // 15x earnings // $ 2.000.000
            Choice("Cars($3.500.000)", "Car Manufacturing"), // 20x earnings // $ 3.500.000                  
            ]
            [Option("Type", "The type of company you want to make.")] string type
             )
        {
            var embed = new DiscordEmbedBuilder();
            int price = 0;
            int operatingcost = 0;
            int earnings = 0;

            if (db.companyexists(name))
            {
                embed.Title = $"Company already exists";
                embed.Color = DiscordColor.Red;
            }
            else
            {
                switch (type)
                {
                    case "ITsupport":
                        earnings = 10;
                        price = 500;
                        operatingcost = 5;
                        break;
                    case "Drugs":
                        earnings = 25;
                        price = 1000;
                        operatingcost = 10;
                        break;
                    case "Farming":
                        earnings = 20;
                        price = 5000;
                        operatingcost = 20;
                        break;
                    case "Education":
                        earnings = 25;
                        price = 10000;
                        operatingcost = 30;
                        break;
                    case "Restaurant":
                        earnings = 30;
                        price = 25000;
                        operatingcost = 40;
                        break;
                    case "Transport":
                        earnings = 50;
                        price = 75000;
                        operatingcost = 50;
                        break;
                    case "Furniture":
                        earnings = 70;
                        price = 100000;
                        operatingcost = 60;
                        break;
                    case "Construction":
                        earnings = 100;
                        price = 150000;
                        operatingcost = 70;
                        break;
                    case "Scamcenter":
                        earnings = 140;
                        price = 350000;
                        operatingcost = 80;
                        break;
                    case "Airline":
                        earnings = 130;
                        price = 750000;
                        operatingcost = 90;
                        break;
                    case "Electronics Manufacturing":
                        earnings = 150;
                        price = 2000000;
                        operatingcost = 100;
                        break;
                    case "Cars":
                        earnings = 200;
                        price = 3500000;
                        operatingcost = 110;
                        break;
                }
                if (db.Userexists(ctx.User.Id))
                {
                    var user = db.Userget(ctx.User.Id);
                    if (user.money >= price)
                    {
                        db.Createcompany(name, ctx.User.Id, type, price, operatingcost, earnings);
                        embed.Title = $"Company {name} made";
                        embed.Color = DiscordColor.Green;
                    }
                    else
                    {
                        embed.Title = $"Not enough money";
                        embed.Color = DiscordColor.Red;
                    }
                }
                else
                {
                    embed.Title = $"You have to have a user first (/startlife)";
                    embed.Color = DiscordColor.Red;
                }

            }
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));

        }

        [SlashCommand("upgrade", "upgrade your company.")]
        public async Task upgradecommand(InteractionContext ctx,
                                                                [
                                                                Choice("Invest", "invest"),
                                                                Choice("Hire NPC worker", "hire"),
                                                                Choice("build new factory", "factory"),
                                                                ]
                                                                [Option("Action","The action you want to take")] string action,
                                                                [Option("Company", "The company you want")] string companyinquestion,
                                                                [Option("Amount", "The company you want")] long amount
                                                                )
        {
            var embed = new DiscordEmbedBuilder();

            if (db.companyexists(companyinquestion))
            {
                var company = db.companyget(companyinquestion);

                if (db.Userexists(ctx.User.Id))
                {
                    var user = db.Userget(ctx.User.Id);

                    if (user.id == company.owner)
                    {
                        int newamount = Convert.ToInt32(amount);

                        if (newamount < 0)
                        {
                            embed.Title = $"Too low of an amount";
                            embed.Color = DiscordColor.Red;
                        }
                        else
                        {
                            bool kill = false;

                            switch (action)
                            {
                                case "invest":
                                    if (user.money >= newamount)
                                    {
                                        company.worth += newamount;
                                        user.money -= newamount;

                                        embed.Title = $"{user.username} invested {newamount} in {companyinquestion}";
                                        embed.Color = DiscordColor.Green;
                                    }
                                    else
                                    {
                                        embed.Title = $"You dont have enough money";
                                        embed.Color = DiscordColor.Red;
                                    }
                                    break;
                                case "hire":
                                    for (int i = 0; i < newamount; i++)
                                    {
                                        if (company.npcworkers < company.factories * 10)
                                        {
                                            if (user.money >= 100)
                                            {
                                                company.npcworkers += 1;
                                                user.money -= 100;
                                            }
                                            else
                                            {
                                                embed.Title = $"You dont have enough money";
                                                embed.Color = DiscordColor.Red;
                                                kill = true;

                                            }
                                        }
                                        else
                                        {
                                            embed.Title = $"You dont have enough factory space";
                                            embed.Color = DiscordColor.Red;
                                            kill = true;

                                        }
                                    }
                                    if (kill)
                                    {
                                        break;
                                    }
                                    embed.Title = $"Upgrade success";
                                    embed.Color = DiscordColor.Green;
                                    break;
                                case "factory":
                                    kill = false;
                                    for (int i = 0; i < newamount; i++)
                                    {
                                        if (user.money >= company.factories * 10000)
                                        {
                                            user.money -= company.factories * 10000;

                                            company.factories += 1;

                                        }
                                        else
                                        {
                                            embed.Title = $"You dont have enough money";
                                            embed.Color = DiscordColor.Red;
                                            kill = true;

                                        }
                                    }
                                    if (kill)
                                    {
                                        break;
                                    }
                                    embed.Title = $"Upgrade success";
                                    embed.Color = DiscordColor.Green;

                                    break;
                            }
                            db.CompanyUpdate(company);
                            db.Userupdate(user);
                        }
                    }
                    else
                    {
                        embed.Title = $"You do not own this company.";
                        embed.Color = DiscordColor.Red;
                    }
                }
                else
                {
                    embed.Title = $"User does not exist.";
                    embed.Color = DiscordColor.Red;
                }
            }
            else
            {
                embed.Title = $"Company dont exist.";
                embed.Color = DiscordColor.Red;
            }
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }
    }
}