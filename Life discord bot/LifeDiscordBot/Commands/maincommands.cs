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
using Discord_bot;
using System.Diagnostics;
using System.Xml.Linq;

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
                db.CreateUserProfile(ctx.User.Id, ctx.User.Username, ctx.User.AvatarUrl, DateTime.Now.AddMonths(1));
                embed.Title = "Life is now started";
                embed.Description = "/work apply company(BotCompany) to get a job";
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
                    embed.AddField("BotCoins", $"{user.botcoin}");
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
                    TimeSpan timethere = DateTime.Now - user.deathtime;
                    embed.AddField("Time till death", timethere.ToString(@"dd\:hh\:mm\:ss"));


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
                    embed.AddField("Npc workers", $"{company.npcworkers} / {company.factories * 10}");
                    embed.AddField("Factories", $"{company.factories}");
                    embed.AddField("Company Worth", $"${company.worth}");

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
                                    user.cut = 50;
                                    embed.Title = $"{user.username} was hired at {user.workplace}.";
                                    embed.Description = $"The default cut is 50%";
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
                                user.cut = 50;
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

                            bool calculatemoney = true;

                            if (company.type == "Drugs" || company.type == "Scamcenter")
                            {

                                Console.WriteLine("at random");
                                Random ran = new();

                                int risk = (int)Math.Round(timethere.TotalSeconds) / 100;

                                if (risk >= 80)
                                {
                                    risk = 80;
                                }

                                int rand = ran.Next(risk, 101);

                                if (rand == 100)
                                {
                                    embed.Title = $"You've been caught! You've been timed out for 1 hour as a penalty";
                                    embed.Color = DiscordColor.Red;

                                    user.location = "prison";

                                    calculatemoney = false;
                                    try
                                    {
                                        var member = (DiscordMember)ctx.User;
                                        await member.TimeoutAsync(DateTime.Now.AddHours(1));
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex);
                                    }

                                    user.time = DateTime.Now;
                                }
                                
                            }
                            
                            if(calculatemoney)
                            {
                                double workerproduction = company.worth / 4294967.294 * Math.Round(timethere.TotalSeconds) * company.npcworkers * company.earnings * 2;

                                double ownerproduction = company.worth / 42949.67294 * Math.Round(timethere.TotalSeconds) * company.earnings / 2;

                                double operationcost = Math.Round(timethere.TotalSeconds) * 0.0116 * company.operatingcost * company.factories;

                                double production = workerproduction + ownerproduction - operationcost;


                                double tax;

                                if (production >= 100000000)
                                {
                                    tax = 0.8;
                                }
                                else if (production >= 10000000)
                                {
                                    tax = 0.7;
                                }
                                else if (production >= 1000000)
                                {
                                    tax = 0.6;
                                }
                                else if (production >= 100000)
                                {
                                    tax = 0.5;
                                }
                                else if (production >= 10000)
                                {
                                    tax = 0.4;
                                }
                                else if (production >= 1000)
                                {
                                    tax = 0.3;
                                }
                                else
                                {
                                    tax = 0.0;
                                }


                                double final = production - (production * tax);
                                user.money += (int)Math.Round(final);

                                embed.Title = $"Moving to {action}";

                                embed.Description = $"You have worked for {timethere.ToString(@"dd\:hh\:mm\:ss")}{Environment.NewLine} You earned: ${Math.Round(ownerproduction, 2)}";
                                embed.Description += $"{Environment.NewLine} Workers earned: ${Math.Round(workerproduction, 2)}";
                                embed.Description += $"{Environment.NewLine} Operation cost: ${Math.Round(operationcost, 2)} {Environment.NewLine} Tax%: {tax * 100} {Environment.NewLine} Tax Cut: ${Math.Round(production * tax, 2)} {Environment.NewLine} Total: ${Math.Round(final)}";

                                embed.Color = DiscordColor.Green;
                                user.location = action;
                                user.time = DateTime.Now;
                            }
                            db.Userupdate(user);
                        }
                        else
                        {
                            TimeSpan timethere = DateTime.Now - user.time;

                            var owneruser = db.Userget(company.owner);

                            bool calculatemoney = true;

                            if (company.type == "Drugs" || company.type == "Scamcenter")
                            {
                                Random ran = new();

                                int risk = (int)Math.Round(timethere.TotalSeconds) / 100;

                                if (risk >= 80)
                                {
                                    risk = 80;
                                }

                                int rand = ran.Next(risk, 101);

                                if (rand == 100)
                                {
                                    embed.Title = $"You've been caught! You've been timed out for 1 hour as a penalty";
                                    embed.Color = DiscordColor.Red;

                                    user.location = "prison";

                                    calculatemoney = false;

                                    try
                                    {
                                        var member = (DiscordMember)ctx.User;
                                        await member.TimeoutAsync(DateTime.Now.AddHours(1));
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex);
                                    }

                                    user.time = DateTime.Now;
                                }
                            }

                            if (calculatemoney)
                            {
                                double userproduction = company.worth / 42949.67294 * Math.Round(timethere.TotalSeconds) * company.earnings;

                                double operationcost = Math.Round(timethere.TotalSeconds) * 0.0116 * company.operatingcost * company.factories;

                                userproduction -= operationcost;
                                double tax;

                                if (userproduction >= 100000000)
                                {
                                    tax = 0.8;
                                }
                                else if (userproduction >= 10000000)
                                {
                                    tax = 0.7;
                                }
                                else if (userproduction >= 1000000)
                                {
                                    tax = 0.6;
                                }
                                else if (userproduction >= 100000)
                                {
                                    tax = 0.5;
                                }
                                else if (userproduction >= 10000)
                                {
                                    tax = 0.4;
                                }
                                else if (userproduction >= 1000)
                                {
                                    tax = 0.3;
                                }
                                else
                                {
                                    tax = 0.0;
                                }

                                userproduction -= userproduction * tax;

                                userproduction -= userproduction / 100 * user.cut;


                                double cut = userproduction / 100 * user.cut;

                                if (cut >= 100000000)
                                {
                                    tax = 0.8;
                                }
                                else if (cut >= 10000000)
                                {
                                    tax = 0.7;
                                }
                                else if (cut >= 1000000)
                                {
                                    tax = 0.6;
                                }
                                else if (cut >= 100000)
                                {
                                    tax = 0.5;
                                }
                                else if (cut >= 10000)
                                {
                                    tax = 0.4;
                                }
                                else if (cut >= 1000)
                                {
                                    tax = 0.3;
                                }
                                else
                                {
                                    tax = 0.0;
                                }

                                double cutfinal = cut - (cut * tax);
                                double final = userproduction - (userproduction * tax);

                                user.money += (int)Math.Round(cutfinal);
                                owneruser.money += (int)Math.Round(final);

                                embed.Title = $"Moving to {action}";
                                embed.Description = $"You have worked for {timethere.ToString(@"dd\:hh\:mm\:ss")} {Environment.NewLine} Earned ${Math.Round(cut)} {Environment.NewLine} Tax%: {tax * 100} {Environment.NewLine} Tax: ${Math.Round(cut * tax)} {Environment.NewLine} Total: ${Math.Round(cutfinal)}";
                                embed.Color = DiscordColor.Green;
                                user.location = action;
                                user.time = DateTime.Now;
                            }
                            
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
            Choice("Cars($3.500.000)", "Cars"), // 20x earnings // $ 3.500.000                  
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
                        operatingcost = 1;
                        break;
                    case "Drugs":
                        earnings = 25;
                        price = 1000;
                        operatingcost = 5;
                        break;
                    case "Farming":
                        earnings = 20;
                        price = 5000;
                        operatingcost = 20;
                        break;
                    case "Education":
                        earnings = 25;
                        price = 10000;
                        operatingcost = 50;
                        break;
                    case "Restaurant":
                        earnings = 30;
                        price = 25000;
                        operatingcost = 150;
                        break;
                    case "Transport":
                        earnings = 50;
                        price = 75000;
                        operatingcost = 750;
                        break;
                    case "Furniture":
                        earnings = 70;
                        price = 100000;
                        operatingcost = 1400;
                        break;
                    case "Construction":
                        earnings = 100;
                        price = 150000;
                        operatingcost = 3000;
                        break;
                    case "Scamcenter":
                        earnings = 140;
                        price = 350000;
                        operatingcost = 9800;
                        break;
                    case "Airline":
                        earnings = 130;
                        price = 750000;
                        operatingcost = 19500;
                        break;
                    case "Electronics Manufacturing":
                        earnings = 150;
                        price = 2000000;
                        operatingcost = 60000;
                        break;
                    case "Cars":
                        earnings = 200;
                        price = 3500000;
                        operatingcost = 140000;
                        break;
                }
                if (db.Userexists(ctx.User.Id))
                {
                    var user = db.Userget(ctx.User.Id);
                    if (user.money >= price)
                    {
                        user.money -= price;
                        db.Userupdate(user);
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
                                                                Choice("Hire NPC worker($100)", "hire"),
                                                                Choice("build new factory($10000*factories)", "factory"),
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
                                        company.worth += newamount / 10;
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
                                                if (!kill)
                                                {
                                                    embed.Title = $"You dont have enough money";
                                                    embed.Description = $"You have hired {i}";
                                                    embed.Color = DiscordColor.Red;
                                                    kill = true;
                                                }

                                            }
                                        }
                                        else
                                        {
                                            if (!kill)
                                            {
                                                embed.Title = $"You dont have enough factory space";
                                                embed.Description = $"You have hired {i}";
                                                embed.Color = DiscordColor.Red;
                                                kill = true;
                                            }

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
                                            if (!kill)
                                            {
                                                embed.Title = $"You dont have enough money";
                                                embed.Description = $"You have bought {i}";
                                                embed.Color = DiscordColor.Red;
                                                kill = true;
                                            }

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

        [SlashCommand("botcoin", "Invest in B-o-T-C-o-I-N")]
        public async Task botcoincommand(InteractionContext ctx,
                                                            [

            Choice("Buy", "buy"),
            Choice("Sell", "sell"),

            ]
            [Option("Choice", "The action you want to do (buy or sell)")] string choice,
            [Option("Amount", "The amount you want to buy or sell")] long? amount = null
             )
        {
            var embed = new DiscordEmbedBuilder();

            int currentprice = db.Stockget();

            if (db.Userexists(ctx.User.Id))
            {

                var user = db.Userget(ctx.User.Id);
                int newamount = 0;
                if (amount != null)
                {
                    newamount = Convert.ToInt32(amount);
                }


                if (choice == "buy" && amount != null)
                {

                    double amounttobuybeforefloor = newamount / currentprice;
                    int amounttobuy = (int)Math.Floor(amounttobuybeforefloor);

                    if (user.money >= amounttobuy * currentprice)
                    {
                        user.money -= amounttobuy * currentprice;
                        user.botcoin += amounttobuy;

                        embed.Title = $"You bought {amounttobuy} botcoins for {amounttobuy * currentprice}";
                        embed.Color = DiscordColor.Green;
                    }
                    else
                    {
                        embed.Title = $"You don't have enough money for this investment.";
                        embed.Color = DiscordColor.Red;
                    }

                }
                else if (choice == "sell" && amount != null)
                {
                    int userstock = user.botcoin;
                    userstock -= newamount;

                    if (userstock < 0)
                    {
                        embed.Title = $"You cant sell more stocks than you have";
                        embed.Color = DiscordColor.Red;
                    }
                    else
                    {
                        user.botcoin = userstock;
                        user.money += currentprice * newamount;
                        embed.Title = $"Successfully sold {newamount} botcoins and got ${currentprice * newamount}";
                        embed.Color = DiscordColor.Green;
                    }


                }
                db.Userupdate(user);

            }
            else
            {
                embed.Title = $"User dont exist";
                embed.Color = DiscordColor.Red;
            }


            if (choice == "see")
            {
                embed.Title = $"The current botcoin price is {currentprice}";
                embed.Color = DiscordColor.Azure;
            }


            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));


        }

        [SlashCommand("setup", "admin command used to setup an account as a bot")]
        public async Task setupcommand(InteractionContext ctx, [Option("User", "The User You want to start as a bot")] DiscordUser giveuser)
        {
            var embed = new DiscordEmbedBuilder();

            if (ctx.User.Id.ToString() == "384367844311433217")
            {
                if (!db.Userexists(giveuser.Id))
                {
                    db.CreateUserProfile(giveuser.Id, giveuser.Username, giveuser.AvatarUrl, DateTime.Now.AddYears(99));
                }
                if (!db.companyexists("BotCompany"))
                {
                    db.Createcompany("BotCompany", giveuser.Id, "ITsupport", 500, 5, 10);
                }

                embed.Title = $"Bot is setup";
                embed.Color = DiscordColor.Gold;
            }
            else
            {
                embed.Title = $"Error no permission to use this command";
                embed.Color = DiscordColor.Red;
            }

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));

            int currentprice = db.Stockget();
            int stockrn;
            DiscordMessage followupMessage = null;
            while (true)
            {
                stockrn = db.Stockget();
                if (stockrn != currentprice)
                {
                    currentprice = stockrn;

                    var followupEmbed = new DiscordEmbedBuilder
                    {
                        Title = "Stock Price Update",
                        Description = $"The stock price has been updated to **{currentprice}**.",
                        Color = DiscordColor.Blue,
                    };
                    if (followupMessage != null)
                    {
                        await ctx.DeleteFollowupAsync(followupMessage.Id);
                    }
                    followupMessage = await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(followupEmbed));

                }
                foreach (var user in db.allusersget())
                {
                    if (DateTime.Now >= user.deathtime)
                    {
                        if (db.companygetofuser(user.id) != null)
                        {
                            db.deletecompaniesofuser(user.id);
                        }

                        db.Createleaderboad(user.id, user.username, user.money, DateTime.Now);

                        db.deleteuser(user.id);

                        embed.Title = $"User {user.username} has been terminated";
                        embed.Description = "Type /startlife to create a new profile";
                        embed.Color = DiscordColor.DarkRed;

                        await ctx.Channel.SendMessageAsync($"<@{user.id}>");

                        await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(embed));
                    }
                    else if (DateTime.Now >= user.deathtime.AddHours(-1))
                    {
                        embed.Title = $"User {user.username} will be terminated in 1 hour";
                        embed.Color = DiscordColor.DarkRed;

                        await ctx.Channel.SendMessageAsync($"<@{user.id}>");
                    }

                    await Task.Delay(1000);
                }
            }
        }

        [SlashCommand("kys", "Terminate yourself")]
        public async Task kyscommand(InteractionContext ctx)
        {
            var embed = new DiscordEmbedBuilder();

            if (db.Userexists(ctx.User.Id))
            {
                var user = db.Userget(ctx.User.Id);
                if (db.companygetofuser(user.id) != null)
                {
                    db.deletecompaniesofuser(user.id);
                }

                db.Createleaderboad(user.id, user.username, user.money, DateTime.Now);

                db.deleteuser(user.id);

                embed.Title = $"User {user.username} has terminated themselves";
                embed.Description = "Type /startlife to create a new profile";
                embed.Color = DiscordColor.DarkRed;
            }
            else
            {
                embed.Title = $"User does not exist, type /startlife to create a profile";
                embed.Color = DiscordColor.Red;
            }

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }

        [SlashCommand("leaderboad", "Shows the leaderboad")]
        public async Task leaderboadcommand(InteractionContext ctx)
        {
            List<LeaderboadUser> list = db.leaderboadget();
            list = list.OrderByDescending(user => user.money).ToList();
            var embed = new DiscordEmbedBuilder();

            embed.Title = $"LeaderBoad";
            embed.Color = DiscordColor.Azure;

            int i = 0;
            foreach (var user in list)
            {
                i++;
                if (i <= list.Count && i <= 10)
                {
                    embed.AddField(user.username, $"${user.money}");
                }

            }
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }

        [SlashCommand("rob", "Rob someone")]
        public async Task robcommand(InteractionContext ctx, [Option("Target", "The user that you want to rob")] DiscordUser giveuser)
        {
            var embed = new DiscordEmbedBuilder();

            if (db.Userexists(ctx.User.Id) && db.Userexists(giveuser.Id))
            {
                var user = db.Userget(ctx.User.Id);

                var target = db.Userget(giveuser.Id);

                Random ran = new();

                int risk = ran.Next(0, 101);

                int robbed = ran.Next(50, 1000);

                if (target.location != "prison")
                {
                    if (user.id != target.id)
                    {
                        if (risk <= 40)
                        {
                            embed.Title = $"You've been caught! You've been timed out for 1 hour as a penalty";
                            embed.Color = DiscordColor.Red;

                            user.location = "prison";

                            try
                            {
                                var member = (DiscordMember)ctx.User;
                                await member.TimeoutAsync(DateTime.Now.AddHours(1));
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                            }
                        }
                        else
                        {
                            if (target.money < robbed)
                            {
                                robbed = target.money;
                            }
                            if (target.money != 0)
                            {
                                embed.Title = $"Robbery successful, you got away with ${robbed}";
                                embed.Color = DiscordColor.Green;
                                user.money += robbed;
                                target.money -= robbed;

                                await ctx.Channel.SendMessageAsync($"<@{target.id}>");

                            }
                            else
                            {
                                embed.Title = $"{target.username} does not have any money to rob";
                                embed.Color = DiscordColor.Red;
                            }
                        }
                    }
                    else
                    {
                        embed.Title = "You cannot rob yourself";
                        embed.Color = DiscordColor.Red;
                    }
                }
                else if (target.location == "prison")
                {
                    embed.Title = $"You cannot use the rob command while target is in prison";
                    embed.Color = DiscordColor.Red;
                }

                db.Userupdate(user);
                db.Userupdate(target);
            }
            else
            {
                embed.Title = $"A user does not exist";
                embed.Color = DiscordColor.Red;
            }
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }

        [SlashCommand("donate", "Donate to someone")]
        public async Task donatecommand(InteractionContext ctx,
            [Option("User", "The user that you want to donate to")] DiscordUser giveuser,
            [Option("Amount", "The amount that you want to donate")] long oldamount)
        {
            var embed = new DiscordEmbedBuilder();

            int amount = Convert.ToInt32(oldamount);

            if (db.Userexists(ctx.User.Id) && db.Userexists(giveuser.Id))
            {
                var user = db.Userget(ctx.User.Id);

                var reciever = db.Userget(giveuser.Id);

                if (user.money >= amount)
                {

                    user.money -= amount;
                    reciever.money += amount;

                    db.Userupdate(user);
                    db.Userupdate(reciever);

                    embed.Title = $"{user.username} donated {amount} to {reciever.username}";
                    embed.Color = DiscordColor.Green;
                }
                else
                {
                    embed.Title = $"You do not have enough money to donate ${amount}";
                    embed.Color = DiscordColor.Red;
                }

            }
            else
            {
                embed.Title = $"A user dont exist";
                embed.Color = DiscordColor.Red;
            }
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }

        [SlashCommand("coinflip", "a coinflip to earn some money")]
        public async Task coinflipcommand(InteractionContext ctx, [Option("Amount", "The amount that you want to donate")] long oldamount)
        {
            var embed = new DiscordEmbedBuilder();

            int amount = Convert.ToInt32(oldamount);

            if (db.Userexists(ctx.User.Id))
            {
                var user = db.Userget(ctx.User.Id);

                Random ran = new();
                int rand = ran.Next(0, 101);

                if (user.money >= amount)
                {
                    if (rand > 50)
                    {
                        user.money += amount;

                        db.Userupdate(user);

                        embed.Title = $"{user.username} won {amount * 2} in your coinflip";
                        embed.Color = DiscordColor.Green;
                    }
                    else
                    {
                        user.money -= amount;

                        db.Userupdate(user);

                        embed.Title = $"You lost {amount}";
                        embed.Color = DiscordColor.Red;
                    }
                }
                else
                {
                    embed.Title = $"You do not have enough money to coinflip ${amount}";
                    embed.Color = DiscordColor.Red;
                }

            }
            else
            {
                embed.Title = $"A user dont exist";
                embed.Color = DiscordColor.Red;
            }
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }
    }
}