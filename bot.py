# bot.py
import datetime
import enum
import json
import os
import time
import typing
import jsonpickle
from datetime import datetime

import discord
from discord import Client, Colour, Interaction, TextChannel, app_commands
from dotenv import load_dotenv

from apscheduler.schedulers.asyncio import AsyncIOScheduler

scheduler = AsyncIOScheduler()

async def print_reminder(mission, client, channel):
    print("reminding...")
    await client.wait_until_ready()
    channel = client.get_channel(channel)
    await channel.send("mission "+mission.op+" is about to start!")
    

load_dotenv()
TOKEN = os.getenv('DISCORD_TOKEN')

intents = discord.Intents.default()
client = discord.Client(intents=intents)
tree = app_commands.CommandTree(client)


class Sides(enum.Enum):
    Blufor = 1
    Opfor = 2
    Independent = 3
    Civilian = 4

class Roles(enum.Enum):
    SquadLeader = 1
    TeamLeader = 2
    RTO= 3
    FO_JTAC = 4
    Medic = 5
    Engineer = 6
    CBRNSpecialist = 7
    AutoRifleman = 8
    AntiTank = 9
    Grenadier = 10
    Sapper = 11
    AntiAir = 12
    Marksman = 13
    Sniper = 14
    RiflemanAT = 15
    Rifleman = 16
    Gunner = 17
    Assistant = 18
    UAVOperator = 19
    Commander = 20
    Driver = 21
    Pilot = 22
    CoPilot = 23
    IndirectFireSpecialist = 24
    VIP = 25

class Response(enum.Enum):
    Yes = 1
    Maybe = 2
    No = 3

class Subdivision:
    name:str
    roles: []

class Division:
    name: str
    subdivisions: []

class Side:
    side: Sides
    divisions: []

class Mission:
    def toJSON(self):
        return json.dumps(self, default=lambda o: o.__dict__, 
            sort_keys=True, indent=4)
    id: int
    campaign: str
    modset: str
    op: str
    date: datetime
    description: str
    channel: int
    sides: []
    

class Reply:
    missionid: int
    response: Response
    side: Sides
    primaryPickId: int
    secondaryPickId: int
    tertiaryPickId: int
    resultPickId: int    

class Player:
    memberid: int
    membername: str
    replies: []
    
class TrackedMessage:
    messageid: int
    channelid: int
    missionid: int

def load_missions():
    # Opening JSON file
    with open('missions.json', 'r') as openfile:
 
        # Reading from json file
        json_str = openfile.read()
        return jsonpickle.decode(json_str)
    
def load_players():
    # Opening JSON file
    with open('players.json', 'r') as openfile:
 
        # Reading from json file
        json_str = openfile.read()
        return jsonpickle.decode(json_str)
    
def load_trackedmessages():
    # Opening JSON file
    with open('trackedmessages.json', 'r') as openfile:
 
        # Reading from json file
        json_str = openfile.read()
        return jsonpickle.decode(json_str)
    
async def update_trackedmessages(client : Client):
    trackedmessages = load_trackedmessages()
    missions = load_missions()
    players = load_players()
    for trackedmessage in trackedmessages:
        for mission in missions:
            if trackedmessage.missionid == mission.id:
                mymission = mission
                print("Mission found")
        channel = await client.fetch_channel(trackedmessage.channelid)
        message = await channel.fetch_message(trackedmessage.messageid)
        myembed=discord.Embed(
            title= "mission "+ mission.op,
            color = Colour.dark_orange()
        )        
        myembed.add_field(name="Campaign",value=mymission.campaign,inline=False)
        myembed.add_field(name="Modset",value=mymission.modset,inline=False)
        myembed.add_field(name="Op",value=mymission.op,inline=False)
        myembed.add_field(name="Date",value=str(mymission.date) + "(<t:"+str(int(time.mktime(mymission.date.timetuple())))+":R>)",inline=False)
        myembed.add_field(name="Description",value=mymission.description,inline=False)
        myembed2=discord.Embed(
            title= "Composition",
            color = Colour.dark_orange()
        )
        tekst = "```"        
        for side in mymission.sides:
            for division in side.divisions:
                tekst = tekst + "" + division.name + ":\n"
                for subdivision in division.subdivisions:
                    tekst = tekst + "    " + subdivision.name + ":\n"
                    for role in subdivision.roles:
                        tekst = tekst + "        " + str(role.name) + "\n"
            tekst = tekst + "```"        
            myembed2.add_field(name=str(side.side.name),value=tekst)
            
        myembed3=discord.Embed(
            title= "RSVPs",
            color = Colour.dark_orange()
        )
        rsvps = 0
        rsvpmaybe = 0
        for player in players:
            myreply : Reply = None
            for reply in player.replies:
                if reply.missionid == mymission.id:
                    myreply = reply
            if myreply is not None:
                if myreply.response == Response.Maybe:
                    rsvpmaybe = rsvpmaybe + 1
                if myreply.response != Response.No:
                    myvalue = ""
                    if myreply.primaryPickId is not None:
                        myvalue = myvalue + Roles(myreply.primaryPickId).name + " "
                    if myreply.secondaryPickId is not None:
                        myvalue = myvalue + Roles(myreply.secondaryPickId).name + " "
                    if myreply.tertiaryPickId is not None:
                        myvalue = myvalue + Roles(myreply.tertiaryPickId).name + " "
                    myembed3.add_field(name=player.membername,value=myvalue,inline=False)
                    rsvps=rsvps+1
        myembed3.add_field(name="Total rsvps", value= str(rsvps),inline=False)
        myembed3.add_field(name="Maybe rsvps", value= str(rsvpmaybe),inline=False)
        myembed3.add_field(name="", value="Please use /respond to reply whether you want to join this mission!",inline=False)
        embedList = []
        embedList.append(myembed)
        embedList.append(myembed2)
        embedList.append(myembed3)
        await message.edit( embeds= embedList )
        
missions = []
missions = load_missions()

for mission in missions:
    scheduler.add_job(print_reminder, 'date', run_date=mission.date, args=[mission, client, mission.channel])

def save_missions(missions: []):
     
    # Serializing json
    json_object = jsonpickle.encode(missions, indent=4)
    
    # Writing to sample.json
    with open("missions.json", "w") as outfile:
        outfile.write(json_object)

def save_players(players: []):
     
    # Serializing json
    json_object = jsonpickle.encode(players, indent=4)
    
    # Writing to sample.json
    with open("players.json", "w") as outfile:
        outfile.write(json_object)

def save_trackedmessages(trackedmessages: []):
     
    # Serializing json
    json_object = jsonpickle.encode(trackedmessages, indent=4)
    
    # Writing to sample.json
    with open("trackedmessages.json", "w") as outfile:
        outfile.write(json_object)

@tree.command(
    name="missioncreate",
    description="Create a new mission",
    guild=discord.Object(id=239086791661977601)
)
@app_commands.default_permissions(manage_channels=True)
async def missioncreate(interaction: Interaction, campaign: str, modset: str, op: str, date: str, channel: TextChannel):
    missions = load_missions()
    newmission = Mission()
    newmission.campaign = campaign
    newmission.modset = modset
    newmission.op = op
    try:
        newmission.date = datetime.strptime(date, '%Y-%m-%d %H:%M')
    except:
        await interaction.response.send_message("Invalid date format. Please supply in YYYY-MM-DD HH:MM e.g. 2024-01-01 12:00")
    else:
        newmission.description = ""
        newmission.sides = []
        newmission.channel = channel.id
        newmission.id = len(missions)
        missions.append(newmission)
        save_missions(missions)
        scheduler.add_job(print_reminder, 'date', run_date=newmission.date, args=[newmission, client, channel.id])
        await interaction.response.send_message("mission created with ID " + str(newmission.id), ephemeral= True)
        trackedmessages = load_trackedmessages()
        myembed=discord.Embed(
            title= "mission "+ op,
            color = Colour.dark_orange()
        )        
        newmessage = await channel.send(embed=myembed)
        newtrackedmessage = TrackedMessage()
        newtrackedmessage.channelid = channel.id
        newtrackedmessage.messageid = newmessage.id
        newtrackedmessage.missionid = newmission.id
        trackedmessages.append(newtrackedmessage)
        save_trackedmessages(trackedmessages)
        await update_trackedmessages(interaction.client)
    
@tree.command(
    name="missionaddside",
    description="Add a side to a mission",
    guild=discord.Object(id=239086791661977601)
)
@app_commands.default_permissions(manage_channels=True)
async def missionaddside(interaction: Interaction, missionid: int, side: Sides):
    missions = load_missions()
    mymission = Mission()
    for mission in missions:
        if missionid == mission.id:
            mymission = mission
            print("Mission found")
    shouldadd = True
    for queryside in mymission.sides:
        if queryside.side is side:
            shouldadd = False
    if(shouldadd):
        newside = Side()
        newside.side = side
        newside.divisions = []
        mymission.sides.append(newside)
    save_missions(missions)
    await update_trackedmessages(interaction.client)
    
    await interaction.response.send_message("Side "+str(side.name)+" added to " + str(mymission.id), ephemeral= True)
    
@tree.command(
    name="missionadddescription",
    description="Add a description to a mission",
    guild=discord.Object(id=239086791661977601)
)
@app_commands.default_permissions(manage_channels=True)
async def missionadddescription(interaction: Interaction, missionid: int, description: str):
    missions = load_missions()
    mymission = Mission()
    for mission in missions:
        if missionid == mission.id:
            mymission = mission
            print("Mission found")
    mymission.description = description
    save_missions(missions)
    await update_trackedmessages(interaction.client)
    
    await interaction.response.send_message("Description added to " + str(mymission.id), ephemeral= True)
    
@tree.command(
    name="missionadddivision",
    description="Add a division to a mission",
    guild=discord.Object(id=239086791661977601)
)
@app_commands.default_permissions(manage_channels=True)
async def missionadddivision(interaction: Interaction, missionid: int, side: Sides, division: str):
    missions = load_missions()
    mymission = Mission()
    for mission in missions:
        if missionid == mission.id:
            mymission = mission
            print("Mission found")
    for queryside in mymission.sides:
        if queryside.side is side:
            myside = queryside
    newdivision = Division()
    newdivision.name = division
    newdivision.subdivisions = []
    myside.divisions.append(newdivision)
    save_missions(missions)
    await update_trackedmessages(interaction.client)
    
    await interaction.response.send_message("Division "+division+" added to " + str(mymission.id), ephemeral= True)
    
@tree.command(
    name="missionaddsubdivision",
    description="Add a division to a mission",
    guild=discord.Object(id=239086791661977601)
)
@app_commands.default_permissions(manage_channels=True)
async def missionaddsubdivision(interaction: Interaction, missionid: int, side: Sides, division: str, subdivision: str):
    missions = load_missions()
    mymission = Mission()
    for mission in missions:
        if missionid == mission.id:
            mymission = mission
            print("Mission found")
    for queryside in mymission.sides:
        if queryside.side is side:
            myside = queryside
            print("side found")
    for querydivision in myside.divisions:
        if querydivision.name == division:
            mydivision = querydivision
    newsubdivision = Subdivision()
    newsubdivision.name = subdivision
    newsubdivision.roles = []
    mydivision.subdivisions.append(newsubdivision)
    save_missions(missions)
    await update_trackedmessages(interaction.client)
    
    await interaction.response.send_message("Subdivision "+subdivision+" added to " + str(mymission.id), ephemeral= True)
    
@tree.command(
    name="missionaddrole",
    description="Add a role to a mission",
    guild=discord.Object(id=239086791661977601)
)
@app_commands.default_permissions(manage_channels=True)
async def missionaddrole(interaction: Interaction, missionid: int, side: Sides, division: str, subdivision: str, role: Roles):
    missions = load_missions()
    mymission = Mission()
    for mission in missions:
        if missionid == mission.id:
            mymission = mission
            print("Mission found")
    for queryside in mymission.sides:
        if queryside.side is side:
            myside = queryside
            print("side found")
    for querydivision in myside.divisions:
        if querydivision.name == division:
            mydivision = querydivision
    for querysubdivision in mydivision.subdivisions:
        if querysubdivision.name == subdivision:
            mysubdivision = querysubdivision
    mysubdivision.roles.append(role)
    save_missions(missions)
    await update_trackedmessages(interaction.client)
    
    await interaction.response.send_message("Role "+str(role.name)+" added to " + str(mymission.id), ephemeral= True)
    
@tree.command(
    name="missionshow",
    description="Show a mission",
    guild=discord.Object(id=239086791661977601)
)
@app_commands.default_permissions(manage_channels=True)
async def missionshow(interaction: Interaction, missionid: int):
    missions = load_missions()
    for mission in missions:
        if missionid == mission.id:
            mymission = mission
    myembed=discord.Embed(
        title= "mission "+ mission.op,
        color = Colour.dark_orange()
    )        
    myembed.add_field(name="ID",value=str(mymission.id),inline=False)
    myembed.add_field(name="Campaign",value=mymission.campaign,inline=False)
    myembed.add_field(name="Modset",value=mymission.modset,inline=False)
    myembed.add_field(name="Op",value=mymission.op,inline=False)
    myembed.add_field(name="Date",value=str(mymission.date) + "(<t:"+str(int(time.mktime(mymission.date.timetuple())))+":R>)",inline=False)
    myembed2=discord.Embed(
        title= "Composition",
        color = Colour.dark_orange()
    )
    tekst = "```"        
    for side in mymission.sides:
        for division in side.divisions:
            tekst = tekst + "" + division.name + ":\n"
            for subdivision in division.subdivisions:
                tekst = tekst + "    " + subdivision.name + ":\n"
                for role in subdivision.roles:
                    tekst = tekst + "        " + str(role.name) + "\n"
        tekst = tekst + "```"        
        myembed2.add_field(name=str(side.side.name),value=tekst)
    embedList = []
    embedList.append(myembed)
    embedList.append(myembed2)
    await interaction.response.send_message( embeds= embedList )
    
@tree.command(
    name="missionlist",
    description="Show all missions",
    guild=discord.Object(id=239086791661977601)
)
@app_commands.default_permissions(manage_channels=True)
async def missionlist(interaction):
    missions = load_missions()
    myembed=discord.Embed(
        title= "missions",
        color = Colour.dark_orange()
    )        
    for mission in missions:
        myembed.add_field(name="",value=str(mission.id) + " " + mission.op + " " + str(mission.date), inline=False)
    await interaction.response.send_message( embed= myembed, ephemeral= True )
    
@tree.command(
    name="respond",
    description="Respond to the most recent op",
    guild=discord.Object(id=239086791661977601)
)
async def respond(interaction: Interaction, rsvp: Response, side: Sides, primaryrole:  typing.Optional[Roles], secondaryrole: typing.Optional[Roles], tertiaryrole: typing.Optional[Roles]):
    players = load_players()
    myplayer = None
    for player in players:
        if interaction.user.id == player.memberid:
            myplayer = player
    if myplayer is None:
        newplayer = Player()
        newplayer.memberid = interaction.user.id
        newplayer.replies = []
        players.append(newplayer)
        myplayer = newplayer
    myplayer.membername = interaction.user.name
    missions = load_missions()
    mymission = None
    for mission in missions:
        if(mission.date > datetime.now()):
            mymission = mission
    if mymission is None:
        myembed=discord.Embed(
            title= "No ops currently active",
            color = Colour.dark_orange()
        )        
        await interaction.response.send_message( embed= myembed )
        return
    myreply = None
    for reply in myplayer.replies:
        if reply.missionid == mymission.id:
            myreply = reply
    if myreply is None:
        myreply = Reply()
        myplayer.replies.append(myreply)
        
    myreply.missionid = mymission.id
    myreply.response = rsvp,
    myreply.side = side,
    if primaryrole is not None:
        print(primaryrole.value)
        myreply.primaryPickId = primaryrole.value
    else:
        myreply.primaryPickId = None
    if secondaryrole is not None:
        myreply.secondaryPickId = secondaryrole.value
    else:
        myreply.secondaryPickId = None
    if tertiaryrole is not None:
        myreply.tertiaryPickId = tertiaryrole.value
    else:
        myreply.tertiaryPickId = None
    
    save_players(players)
    
    await update_trackedmessages(interaction.client)
    
    myembed=discord.Embed(
        title= "You have replied!",
        color = Colour.dark_orange()
    )        
    myembed.add_field(name="Mission",value=mission.op, inline=False)
    myembed.add_field(name="Response",value=str(rsvp), inline=False)
    myembed.add_field(name="Side",value=str(side), inline=False)
    if primaryrole is not None:
        myembed.add_field(name="Primary pick",value=str(primaryrole.name), inline=False)
    if secondaryrole is not None:
        myembed.add_field(name="Secondary pick",value=str(secondaryrole.name), inline=False)
    if tertiaryrole is not None:
        myembed.add_field(name="Tertiary pick",value=str(tertiaryrole.name), inline=False)
    await interaction.response.send_message( embed= myembed, ephemeral= True )
    
@tree.command(
    name="armabot",
    description="Get info about arma bot",
    guild=discord.Object(id=239086791661977601)
)
async def armabot(interaction: Interaction):
    myembed=discord.Embed(
        title= "ARMA Bot Current commands",
        color = Colour.dark_orange()
    )        
    myembed.add_field(name="/missioncreate",value="Creates a new mission", inline=False)
    myembed.add_field(name="/missionadddescription",value="Adds a description to a created mission", inline=False)
    myembed.add_field(name="/missionaddside",value="Adds a side (e.g. Blufor) to a created mission", inline=False)
    myembed.add_field(name="/missionadddivision",value="Adds a division to a side for a created mission", inline=False)
    myembed.add_field(name="/missionaddsubdivision",value="Adds a subdivision to a division for a created mission", inline=False)
    myembed.add_field(name="/missionaddrole",value="Adds a role to a subdivision for a created mission", inline=False)
    myembed.add_field(name="/missionshow",value="Show a mission. Be aware that this does not get automatically updated (yet)", inline=False)
    myembed.add_field(name="/missionlist",value="Get a list of all missions in the database", inline=False)
    myembed.add_field(name="/respond",value="Allows players to respond to the mission. This assumes there's only 1 mission in the future.", inline=False)
    myembed2=discord.Embed(
        title= "ARMA Bot To Do List",
        color = Colour.dark_orange()
    )        
    myembed2.add_field(name="",value="Setup the Guild ID as an environment variable, so it can be used on other servers", inline=False)
    myembed2.add_field(name="",value="Remove the 'No' rsvp, it really doesn't add anything and makes things more complicated.", inline=False)
    myembed2.add_field(name="",value="There is no error handling if you fill in something that doesn't exist (e.g. missions, sides, divisions)", inline=False)
    myembed2.add_field(name="",value="Specifically look at no response responses.", inline=False)
    myembed2.add_field(name="",value="You can not yet delete sides, divisions, subdivisions or roles.", inline=False)
    myembed2.add_field(name="",value="Don't accept duplicate divisions and subdivisions", inline=False)
    myembed2.add_field(name="",value="If a role is added to a division or subdivision that doesn't exist, make it.", inline=False)
    myembed2.add_field(name="",value="The respond command does not check whether the roles chosen are actually in the mission.", inline=False)
    myembed2.add_field(name="",value="Add an optional Description parameter for mission create.", inline=False)
    myembed2.add_field(name="",value="Add an optional OP ID for respond.", inline=False)
    myembed2.add_field(name="",value="The mission is posted immediately after doing /missioncreate. I should add a /missionpost or something to only post when the mission is completely filled in, to prevent confusion", inline=False)
    myembed2.add_field(name="",value="There's only a reminder at the OP time, not 30 minutes in advance", inline=False)
    myembed2.add_field(name="",value="There is no @group which pings all of the group.", inline=False)
    myembed2.add_field(name="",value="It doesn't work correctly if there are multiple future missions", inline=False)
    myembed2.add_field(name="",value="Investigate the possibility of adding multiple roles in the same /missionaddrole command, so it becomes less tedious to set up", inline=False)
    myembed2.add_field(name="",value="I have an idea to add a suggested setup, based on primaries/secondaries/tertiaries", inline=False)
    embedList = []
    embedList.append(myembed)
    embedList.append(myembed2)
    await interaction.response.send_message( embeds= embedList)


@client.event
async def on_ready():
    await tree.sync(guild=discord.Object(id=239086791661977601))
    scheduler.start()
    print("Ready!")

client.run(TOKEN)
