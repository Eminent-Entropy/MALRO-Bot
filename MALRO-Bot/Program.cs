using System;
using System.Threading.Tasks;
using System.Reflection;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using OAuth2;

public class Program
{
    private CommandService commands;
    private DiscordSocketClient client;
    private IServiceProvider services;

    public string mainDir = "C://MALRO-Bot";
    public string trasferDir = "transfer";

    public Program()
    {
        
    }

    static void Main(string[] args) => new Program().Start().GetAwaiter().GetResult();

    public async Task Start()
    {
        client = new DiscordSocketClient();
        commands = new CommandService();
        string token = "redacted";
        //client.UserJoined += AnnounceJoinedUser; //Check if userjoined
        client.UserLeft += AnnounceUserLeft;
        services = new ServiceCollection()
                .BuildServiceProvider();

        await InstallCommands();

        await client.LoginAsync(TokenType.Bot, token);
        await client.StartAsync();

        string transferPath = Path.Combine(mainDir, trasferDir);
        if (!Directory.Exists(mainDir)) Directory.CreateDirectory(mainDir);
        if (!Directory.Exists(transferPath)) Directory.CreateDirectory(transferPath);

        await threeHour();

        await Task.Delay(-1);
    }

    public async Task InstallCommands()
    {
        // Hook the MessageReceived Event into our Command Handler
        client.MessageReceived += HandleCommand;
        // Discover all of the commands in this assembly and load them.
        await commands.AddModulesAsync(Assembly.GetEntryAssembly());
    }

    public async Task HandleCommand(SocketMessage messageParam)
    {
        // Don't process the command if it was a System Message
        var message = messageParam as SocketUserMessage;
        if (message == null) return;
        // Create a number to track where the prefix ends and the command begins
        int argPos = 0;
        // Determine if the message is a command, based on if it starts with '!' or a mention prefix
        if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(client.CurrentUser, ref argPos))) return;
        // Create a Command Context
        var context = new CommandContext(client, message);
        // Execute the command. (result does not indicate a return value, 
        // rather an object stating if the command executed successfully)
        var result = await commands.ExecuteAsync(context, argPos, services);
        if (!result.IsSuccess)
            await context.Channel.SendMessageAsync(result.ErrorReason);
    }

    /*public async Task AnnounceJoinedUser(SocketGuildUser user) //welcomes New Players
    {
        SocketGuild guild = user.Guild;
        SocketTextChannel channel = guild.GetChannel(380757221476139021) as SocketTextChannel;
        await channel.SendMessageAsync(user.Mention + " https://cdn.discordapp.com/attachments/380757221476139021/380836053407367168/image.gif");
    }*/

    public async Task AnnounceUserLeft(SocketGuildUser user) //notifies of a user leaving the server
    {
        SocketGuild guild = user.Guild;
        SocketTextChannel channel = guild.GetChannel(466373989854347264) as SocketTextChannel;
        await channel.SendMessageAsync("***" + user.Username + " has left the server***");
    }

    public async Task threeHour()
    {
        while (true)
        {
            await killCheck();
            await Task.Delay(1000 * 60 * 60 * 3); //three hours
        }
    }

    public async Task killCheck()
    {
        //WebRequest request = WebRequest.Create("https://esi.tech.ccp.is/latest/wars/");
        
    }
}

public class Commands : ModuleBase
{
    public string mainDir = "C://MALRO-Bot";
    public string trasferDir = "transfer";

    ulong leadership = 368900184329158660;
    ulong offier = 421350014783324167;
    //ulong member = 466320045606436874;
    //ulong allies = 466395765422686209;
    //ulong everyone = 466376274617565204;

    /**
     * Ping test
     */
    [Command("ping"), Summary("Test bot ping")]
    public async Task ping()
    {
        await Context.User.SendMessageAsync("pong");
    }

    /**
     * Gives list of discord users sorted by roles
     */
    public async Task userlist(bool everyone)
    {
        SocketGuild Guild = Context.Guild as SocketGuild;
        SocketGuildUser User = Context.User as SocketGuildUser;
        if ((User.Roles.Contains(Guild.GetRole(leadership) as SocketRole) || User.Roles.Contains(Guild.GetRole(offier) as SocketRole)))
        {
            List<SocketRole> roleList = Guild.Roles.ToList();
            List<List<SocketGuildUser>> roles = new List<List<SocketGuildUser>>();
            //for (int i = 0; i < roleList.Count(); i++)
            //roles.Add(new List<SocketGuildUser>());

            SocketRole[] orderedRoles = new SocketRole[5];
            for (int i = 0; i < roleList.Count(); i++)
            {
                if (roleList[i].Name == "Leadership") orderedRoles[0] = roleList[i];
                if (roleList[i].Name == "Officer") orderedRoles[1] = roleList[i];
                if (roleList[i].Name == "member") orderedRoles[2] = roleList[i];
                if (roleList[i].Name == "Allies") orderedRoles[3] = roleList[i];
                if (roleList[i].Name == "@everyone") orderedRoles[4] = roleList[i];
            }
            string output = "";
            string output2 = "";
            for (int i = 0; i < orderedRoles.Count(); i++)
            {

                if (orderedRoles[i].Name == "@everyone")
                    output2 += "**" + "no role" + "**" + Environment.NewLine;
                else
                    output += "**" + orderedRoles[i].Name + "**" + Environment.NewLine;

                List<SocketGuildUser> userList = orderedRoles[i].Members.ToList();

                for (int a = 0; a < userList.Count(); a++)
                {
                    List<SocketRole> OrRoleL = orderedRoles.ToList();
                    if ((i == 2 && (orderedRoles[0].Members.Contains(userList[a]) || orderedRoles[1].Members.Contains(userList[a])) ||
                        (i == 4 && (orderedRoles[0].Members.Contains(userList[a]) || orderedRoles[2].Members.Contains(userList[a]) || orderedRoles[3].Members.Contains(userList[a])))))
                    {

                    }
                    else if (i == 4)
                    {
                        if (userList[a].Nickname == null)
                            output2 += userList[a].Username + Environment.NewLine;
                        else
                            output2 += userList[a].Nickname + Environment.NewLine;
                    }
                    else
                    {
                        if (userList[a].Nickname == null)
                            output += userList[a].Username + Environment.NewLine;
                        else
                            output += userList[a].Nickname + Environment.NewLine;
                    }
                }
                output += Environment.NewLine;
            };
            SocketTextChannel channel = Guild.GetTextChannel(466373989854347264);
            await channel.SendMessageAsync(output);
            if (everyone) await channel.SendMessageAsync(output2);
        }
        else await Context.Channel.SendMessageAsync("You don't have the correct roles");
    }

    [Command("listusers"), Summary("lists members")]
    public async Task listusers()
    {
        await userlist(false);
    }

    [Command("listusers"), Summary("lists members")]
    public async Task listusersEveryone(string everyone)
    {
        if (everyone == "everyone")
        {
            await userlist(true);
        }
        else
        {
            await Context.Channel.SendMessageAsync("Incorrect input");
        }
    }

    /**
     * meme
     */
    [Command("hotpockets")]
    public async Task hotPockets()
    {
        await Context.Channel.SendFileAsync("D:\\gerbster\\Pictures\\image.jpg");
    }

    /**
     * member requests transfer
     */
    [Command("transfer")]
    public async Task transfer(string name)
    {
        await transferTask(name);
    }

    [Command("transfer")]
    public async Task transfer(string firstName, string lastName)
    {
        string name = firstName + " " + lastName;
        await transferTask(name);
    }

    [Command("transfer")]
    public async Task transfer(string firstName, string middleName, string lastName)
    {
        string name = firstName + " " + middleName + " " + lastName;
        await transferTask(name);
    }

    public async Task transferTask(string name)
    {
        string transferPath = Path.Combine(mainDir, trasferDir);
        SocketGuild Guild = Context.Guild as SocketGuild;
        SocketTextChannel channel = Guild.GetChannel(466373989854347264) as SocketTextChannel;
        SocketGuildUser user = Context.User as SocketGuildUser;
        string username = "";
        if (user.Nickname == null)
            username = user.Username;
        else
            username = user.Nickname;

        string userFile = Path.Combine(transferPath, username);
        if (!File.Exists(userFile))
        {
            File.Create(userFile).Close();
            File.WriteAllText(userFile, user.Id.ToString());
        }

        await channel.SendMessageAsync("Transfer " + name + " - Requested by " + username);
    }

    /**
     * leadership/officer tell member invite is sent
     */
    [Command("trf")]
    public async Task trf(string name)
    {
        await trfTask(name);
    }

    [Command("trf")]
    public async Task trf(string firstname, string lastname)
    {
        string name = firstname + " " + lastname;
        await trfTask(name);
    }

    [Command("trf")]
    public async Task trf(string firstname, string middlename, string lastname)
    {
        string name = firstname + " " + middlename + " " + lastname;
        await trfTask(name);
    }

    public async Task trfTask(string name)
    {
        SocketGuild Guild = Context.Guild as SocketGuild;
        SocketTextChannel channel = Guild.GetChannel(466373989854347264) as SocketTextChannel;
        string transferPath = Path.Combine(mainDir, trasferDir);
        string userFile = Path.Combine(transferPath, name);

        if (File.Exists(userFile))
        {
            ulong userID = Convert.ToUInt64(File.ReadAllText(userFile));
            IUser user = await Context.Client.GetUserAsync(userID);
            await user.SendMessageAsync("Invite to corp sent 🦆");
            File.Delete(userFile);
            await channel.SendMessageAsync("Message sent");
        }
        else
            await channel.SendMessageAsync("User has not requested a transfer");
    }

    [Command("evetime")]
    public async Task evetime()
    {
        int utcMin = DateTime.UtcNow.Minute;
        string utcMinS;
        if (utcMin < 10)
            utcMinS = "0" + utcMin.ToString();
        else
            utcMinS = utcMin.ToString();
        string utcTime = DateTime.UtcNow.TimeOfDay.Hours.ToString() + ":" + utcMinS;
        await Context.Channel.SendMessageAsync("It is " + utcTime + " in eve time");
    }

    [Command("evetime")]
    public async Task evetimeCount(int time, string type)
    {
        if (type == "min")
        {
            int utcMin = DateTime.UtcNow.AddMinutes(time).Minute;
            string utcMinS = "";
            if (utcMin < 10) utcMinS = "0" + utcMin.ToString();
            else utcMinS = utcMin.ToString();
            string utcTime = DateTime.UtcNow.TimeOfDay.Hours.ToString() + ":" + utcMinS;
            if (time != 1)
                await Context.Channel.SendMessageAsync("It will be " + utcTime + " in " + time + " minutes");
            else
                await Context.Channel.SendMessageAsync("It will be " + utcTime + " in " + time + " minute");
        }
        if (type == "hours" || type == "hour")
        {
            int utcMin = DateTime.UtcNow.Minute;
            string utcMinS = "";
            if (utcMin < 10) utcMinS = "0" + utcMin.ToString();
            else utcMinS = utcMin.ToString();
            string utcTime = DateTime.UtcNow.AddHours(time).Hour + ":" + utcMinS;
            if (time != 1)
                await Context.Channel.SendMessageAsync("It will be " + utcTime + " in " + time + " hours");
            else
                await Context.Channel.SendMessageAsync("It will be " + utcTime + " in " + time + " hour");
        }
    }

    [Command("dt")]
    public async Task dt()
    {
        int utcHour = DateTime.UtcNow.Hour;
        int utcMin = DateTime.UtcNow.Minute;
        if (utcHour == 11 && utcMin >= 0 && utcMin <= 30)
        {
            string time = (30 - utcMin).ToString();
            await Context.Channel.SendMessageAsync("Server is in daily downtime, normal uptime starts in " +  time + " Minutes");
        }
        else
        {
            string timeH = "";
            if (utcHour < 10)
                timeH = (11 - utcHour - 1).ToString();
            else if (utcHour >= 11)
                timeH = (24 - utcHour + 11 - 1).ToString();
            else timeH = "0";

            string timeM = (60 - utcMin).ToString();

            string M;
            if (timeM != "1")
                M = "minutes";
            else
                M = "minute";
            string H;
            if (timeH != "1")
                H = "hours";
            else
                H = "hour";

            await Context.Channel.SendMessageAsync("Daily downtime will occur in " + timeH + H + " " + timeM + M);
            
        }
    }
}