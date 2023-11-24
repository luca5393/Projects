using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_bot.profilesystem
{
    public class coinsstore 
    {
        public string path = @"C:\Users\luca5393\source\repos\Discord bot\Discord bot\profilesystem\UserInfo.json";
        public bool Userdetails(DUser user)
        {
            try
            {

                var json = File.ReadAllText(path);
                var jsonObj = JObject.Parse(json);


                var members = jsonObj["members"].ToObject<List<DUser>>();
                members.Add(user);

                jsonObj["members"] = JArray.FromObject(members);
                File.WriteAllText(path, jsonObj.ToString());


                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool Userexists(string username)
        {
            using (StreamReader sr = new StreamReader(@"C:\Users\luca5393\source\repos\Discord bot\Discord bot\profilesystem\UserInfo.json"))
            {
                string json = sr.ReadToEnd();
                jsonfile userget = JsonConvert.DeserializeObject<jsonfile>(json);

                foreach( var user in userget.members) 
                { 
                    if(user.Username == username)
                    {
                        return true;
                    } 
                }
                return false;
            }
        }

        public bool companyexists(string username, string bus)
        {
            using (StreamReader sr = new StreamReader(@"C:\Users\luca5393\source\repos\Discord bot\Discord bot\profilesystem\UserInfo.json"))
            {
                string json = sr.ReadToEnd();
                jsonfile userget = JsonConvert.DeserializeObject<jsonfile>(json);

                foreach (var user in userget.company)
                {
                    if (user.owner.Username == username && user.bustype == bus)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool loanexists(DUser loaner, DUser banker)
        {
            using (StreamReader sr = new StreamReader(@"C:\Users\luca5393\source\repos\Discord bot\Discord bot\profilesystem\UserInfo.json"))
            {
                string json = sr.ReadToEnd();
                jsonfile userget = JsonConvert.DeserializeObject<jsonfile>(json);

                foreach (var loan in userget.loans)
                {
                    if (loan.Loaner.Username == loaner.Username && loan.Banker.Username == banker.Username)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public void Useraddremovecoins(string username, int coins)
        {

            var json = File.ReadAllText(path);
            var jsonObj = JObject.Parse(json);

            var members = jsonObj["members"].ToObject<List<DUser>>();
            foreach(DUser user in members)
            {
                if(user.Username == username)
                {
                    user.Coins = user.Coins + coins;
                }
            }
            
            jsonObj["members"] = JArray.FromObject(members);

            File.WriteAllText(path, jsonObj.ToString());

        }

        public DUser GetUser(string username)
        {
            using (StreamReader sr = new StreamReader(@"C:\Users\luca5393\source\repos\Discord bot\Discord bot\profilesystem\UserInfo.json"))
            {
                string json = sr.ReadToEnd();
                jsonfile userget = JsonConvert.DeserializeObject<jsonfile>(json);

                foreach (var user in userget.members)
                {
                    if (user.Username == username)
                    {
                        return new DUser() {
                        
                            Username = user.Username,
                            Coins = user.Coins,
                            avatarurl = user.avatarurl
                        };
                    }
                }
                return null;
            }
        }

        public Company Getcompany(string username, string bus)
        {
            using (StreamReader sr = new StreamReader(@"C:\Users\luca5393\source\repos\Discord bot\Discord bot\profilesystem\UserInfo.json"))
            {
                string json = sr.ReadToEnd();
                jsonfile userget = JsonConvert.DeserializeObject<jsonfile>(json);

                foreach (var user in userget.company)
                {
                    if (user.owner.Username == username && user.bustype == bus)
                    {
                        return user;
                        
                    }
                }
                return null;
            }
        }

        public Loan Getloan(DUser loaner, DUser banker)
        {
            using (StreamReader sr = new StreamReader(@"C:\Users\luca5393\source\repos\Discord bot\Discord bot\profilesystem\UserInfo.json"))
            {
                string json = sr.ReadToEnd();
                jsonfile userget = JsonConvert.DeserializeObject<jsonfile>(json);

                foreach (var loan in userget.loans)
                {
                    if (loan.Loaner.Username == loaner.Username && loan.Banker.Username == banker.Username)
                    {
                        return new Loan { 
                            Loaner = loan.Loaner,
                            Banker= loan.Banker,
                            Time  = loan.Time,
                            Coins = loan.Coins,
                            active = loan.active,
                            Payback = loan.Payback

                        };

                    }
                }
                return null;
            }
        }

        public bool loanrequest(DUser loaner, DUser banker, int time, int money, int payback)
        {
            try
            {
                var json = File.ReadAllText(path);
                var jsonObj = JObject.Parse(json);


                var loans = jsonObj["loans"].ToObject<List<Loan>>();
                

                Loan newloan = new Loan() 
                { 
                    Loaner = loaner,
                    Banker = banker,
                    Time = time,
                    Coins = money,
                    Payback = payback,
                    active  = false
                };

                
                loans.Add(newloan);

                jsonObj["loans"] = JArray.FromObject(loans);
                File.WriteAllText(path, jsonObj.ToString());


                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public async void loanend(Loan loan, InteractionContext ctx, DiscordUser muteuser)
        {
            var json = File.ReadAllText(path);
            var jsonObj = JObject.Parse(json);


            var loans = jsonObj["loans"].ToObject<List<Loan>>();

           foreach(Loan item in loans)
            {
                if(item.Banker.Username == loan.Banker.Username && item.Loaner.Username == loan.Loaner.Username)
                {
                    item.active = true;
                }
            }

            jsonObj["loans"] = JArray.FromObject(loans);
            File.WriteAllText(path, jsonObj.ToString());

            await Task.Delay(loan.Time*1000);

            var loaner = GetUser(loan.Loaner.Username);
            int price = loan.Coins + (int)Math.Round(loan.Coins * (loan.Payback / 100.0));

            if (loaner.Coins >= price)
            {
                Useraddremovecoins(loan.Loaner.Username, -price);
               
                Useraddremovecoins(loan.Banker.Username, price);

                quickemblem(false, ctx,
                    "Loan Complete",
                    $"The Loan between {loan.Loaner.Username} and {loan.Banker.Username} has completed",
                    DiscordColor.Green);

                json = File.ReadAllText(path);
                jsonObj = JObject.Parse(json);

                Loan item = loan;

                int indexToRemove = loans.FindIndex(loan =>
                    loan.Loaner.Username == item.Loaner.Username &&
                    loan.Banker.Username == item.Banker.Username);

                if (indexToRemove != -1)
                {
                    loans.RemoveAt(indexToRemove);
                }

                jsonObj["loans"] = JArray.FromObject(loans);
                File.WriteAllText(path, jsonObj.ToString());
            }
            else
            {
                int muteDuration = (price - loaner.Coins) * 2;
                quickemblem(false, ctx,
                    "Loan Complete",
                    $"The Loan between {loan.Loaner.Username} and {loan.Banker.Username} has completed. But {loan.Loaner.Username} dont have enough money and {loan.Banker.Username} only gets {loaner.Coins}",
                    DiscordColor.Green);
                quickemblem(false, ctx, "Loan not paid", $"{loan.Loaner.Username} will recive a mute for {muteDuration} seconds", DiscordColor.Red);

                

                Useraddremovecoins(loan.Banker.Username, loaner.Coins);
                Useraddremovecoins(loan.Loaner.Username, -loaner.Coins);
                json = File.ReadAllText(path);
                jsonObj = JObject.Parse(json);
                Loan item = loan;

                int indexToRemove = loans.FindIndex(loan =>
                    loan.Loaner.Username == item.Loaner.Username &&
                    loan.Banker.Username == item.Banker.Username);

                if (indexToRemove != -1)
                {
                    loans.RemoveAt(indexToRemove);
                }

                jsonObj["loans"] = JArray.FromObject(loans);
                File.WriteAllText(path, jsonObj.ToString());

                var timeduration = DateTime.Now + TimeSpan.FromSeconds(muteDuration);
                var member = (DiscordMember)muteuser;
                await member.TimeoutAsync(timeduration);

                
            }

        }
        public async void quickemblem(bool svar, InteractionContext ctx, string title, string  des, DiscordColor color)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = title,
                Description = des,
                Color = color
            };
            if (svar)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
            }
            else
            {
                await ctx.Channel.SendMessageAsync(embed: embed);
            }
        }

        public void companyupgrade(Company company, string upgrade)
        {

            var json = File.ReadAllText(path);
            var jsonObj = JObject.Parse(json);

            var members = jsonObj["company"].ToObject<List<Company>>();
            foreach (Company comp in members)
            {
                if (comp.owner.Username == company.owner.Username && comp.bustype == company.bustype)
                {
                    switch (upgrade)
                    {
                        case "worker":
                            comp.workers++;
                            break;
                        case "generation":
                            comp.coingeneration = comp.coingeneration + (int)Math.Round((double)comp.coingeneration / 100 * 25);
                            comp.coingenerationupgrade++;
                            break;
                        case "operating":
                            comp.operatingcost = (int)Math.Round(comp.operatingcost * 0.95);
                            comp.operationupgrades++;
                            break;
                        
                    }
                }
            }

            jsonObj["company"] = JArray.FromObject(members);

            File.WriteAllText(path, jsonObj.ToString());

        }


    }

        class jsonfile
    {
        public DUser[] members { get; set;}
        public Loan[] loans { get; set;}
        public Company[] company { get; set; }

    }
}
