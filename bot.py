# bot.py
import datetime
import enum
import json
import os
import time
import typing
import jsonpickle
from datetime import datetime, timedelta

import discord
from discord import Client, Colour, Interaction, Role, TextChannel, User, app_commands
from dotenv import load_dotenv

from apscheduler.schedulers.asyncio import AsyncIOScheduler

scheduler = AsyncIOScheduler()

async def print_reminder(mission, client, channel):
    print("reminding...")
    await client.wait_until_ready()
    channel = client.get_channel(channel)
    myembed=discord.Embed(
        title= "mission "+ mission.op + " will start in 30 minutes!",
        color = Colour.dark_orange()
    )        
    await channel.send(embed=myembed)
    

load_dotenv()
TOKEN = os.getenv('DISCORD_TOKEN')
GUILDID = os.getenv('GUILD_ID')

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

class Subdivision:
    name:str
    roles: typing.List[Roles]

class Division:
    name: str
    subdivisions: typing.List[Subdivision]

class Side:
    side: Sides
    divisions: typing.List[Division]

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
    usertoping: int
    sides: typing.List[Side]
    

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
    replies: typing.List[Reply]
    
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
    
async def update_trackedmessages(client : Client, mymission : Mission):
    trackedmessages = load_trackedmessages()
    missions = load_missions()
    players = load_players()
    for trackedmessage in trackedmessages:
        if trackedmessage.missionid == mymission.id:
            print("Mission found for update")
            channel = await client.fetch_channel(trackedmessage.channelid)
            message = await channel.fetch_message(trackedmessage.messageid)
            myembed=discord.Embed(
                title= "mission "+ mymission.op,
                color = Colour.dark_orange()
            )        
            myembed.add_field(name="Campaign",value=mymission.campaign,inline=False)
            myembed.add_field(name="Modset",value=mymission.modset,inline=False)
            myembed.add_field(name="Op",value=mymission.op,inline=False)
            myembed.add_field(name="Date",value="<t:"+str(int(time.mktime(mymission.date.timetuple())))+"> " + "(<t:"+str(int(time.mktime(mymission.date.timetuple())))+":R>)",inline=False)
            myembed.add_field(name="Description",value=mymission.description,inline=False)
            myembed2=discord.Embed(
                title= "Composition",
                color = Colour.dark_orange()
            )
            for side in mymission.sides:
                tekst = "```"        
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
            await message.edit(content="<@" + str(mymission.usertoping) + "> ", embeds= embedList )
        
missions = []
missions = load_missions()

for mission in missions:
    scheduler.add_job(print_reminder, 'date', run_date=mission.date - timedelta(minutes=30), args=[mission, client, mission.channel])

def save_missions(missions: typing.List[Mission]):
     
    # Serializing json
    json_object = jsonpickle.encode(missions, indent=4)
    
    # Writing to sample.json
    with open("missions.json", "w") as outfile:
        outfile.write(json_object)

def save_players(players: typing.List[Player]):
     
    # Serializing json
    json_object = jsonpickle.encode(players, indent=4)
    
    # Writing to sample.json
    with open("players.json", "w") as outfile:
        outfile.write(json_object)

def save_trackedmessages(trackedmessages: typing.List[TrackedMessage]):
     
    # Serializing json
    json_object = jsonpickle.encode(trackedmessages, indent=4)
    
    # Writing to sample.json
    with open("trackedmessages.json", "w") as outfile:
        outfile.write(json_object)

@tree.command(
    name="missioncreate",
    description="Create a new mission",
    guild=discord.Object(id=GUILDID)
)
@app_commands.default_permissions(manage_channels=True)
async def missioncreate(interaction: Interaction, campaign: str, modset: str, op: str, date: str, channel: TextChannel, usertoping: Role, description: typing.Optional[str]):
    missions = load_missions()
    newmission = Mission()
    newmission.campaign = campaign
    newmission.modset = modset
    newmission.op = op
    newmission.usertoping = usertoping.id
    try:
        newmission.date = datetime.strptime(date, '%Y-%m-%d %H:%M')
    except:
        await interaction.response.send_message("Invalid date format. Please supply in YYYY-MM-DD HH:MM e.g. 2024-01-01 12:00", ephemeral= True )
    else:
        if description != None:
            newmission.description = description
        else:
            newmission.description = ""
        newmission.sides = []
        newmission.channel = channel.id
        newmission.id = len(missions)
        missions.append(newmission)
        save_missions(missions)
        await interaction.response.send_message("mission created with id " + str(newmission.id), ephemeral= True)

@tree.command(
    name="missionpost",
    description="Post a created mission",
    guild=discord.Object(id=GUILDID)
)
@app_commands.default_permissions(manage_channels=True)
async def missionpost(interaction: Interaction, missionid: int):
    missions = load_missions()
    mymission = None
    for mission in missions:
        if missionid == mission.id:
            mymission = mission
            print("Mission found for post")
            break
    if mymission == None:
        await interaction.response.send_message("Mission " + str(missionid) + " not found", ephemeral= True)
        return
    channel = await client.fetch_channel(mymission.channel)
    scheduler.add_job(print_reminder, 'date', run_date=mymission.date - timedelta(minutes=30), args=[mymission, client, channel.id])
    await interaction.response.send_message("mission post created for " + str(mymission.id), ephemeral= True)
    trackedmessages = load_trackedmessages()
    myembed=discord.Embed(
        title= "mission "+ mymission.op,
        color = Colour.dark_orange()
    )        
    newmessage = await channel.send(content="<@" + str(mymission.usertoping) + "> ", embed=myembed)
    newtrackedmessage = TrackedMessage()
    newtrackedmessage.channelid = channel.id
    newtrackedmessage.messageid = newmessage.id
    newtrackedmessage.missionid = mymission.id
    trackedmessages.append(newtrackedmessage)
    save_trackedmessages(trackedmessages)
    await update_trackedmessages(interaction.client, mymission)

    
@tree.command(
    name="missionaddside",
    description="Add a side to a mission",
    guild=discord.Object(id=GUILDID)
)
@app_commands.default_permissions(manage_channels=True)
async def missionaddside(interaction: Interaction, missionid: int, side: Sides):
    missions = load_missions()
    mymission = None
    for mission in missions:
        if missionid == mission.id:
            mymission = mission
            print("Mission found for addside")
            break
    if mymission == None:
        await interaction.response.send_message("Mission " + str(missionid) + " not found", ephemeral= True)
        return
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
    await interaction.response.send_message("Side "+str(side.name)+" added to " + str(mymission.id), ephemeral= True)
    await update_trackedmessages(interaction.client, mymission)
    
    
@tree.command(
    name="missiondeleteside",
    description="Delete a side to a mission",
    guild=discord.Object(id=GUILDID)
)
@app_commands.default_permissions(manage_channels=True)
async def missiondeleteside(interaction: Interaction, missionid: int, side: Sides):
    missions = load_missions()
    mymission = None
    for mission in missions:
        if missionid == mission.id:
            mymission = mission
            print("Mission found")
    if mymission == None:
        await interaction.response.send_message("Mission " + str(missionid) + " not found", ephemeral= True)
        return
    myside = None
    for queryside in mymission.sides:
        if queryside.side is side:
            myside = queryside
            print("side found")
    if myside == None:
        await interaction.response.send_message("Side " + str(side) + " not found", ephemeral= True)
        return
    mymission.sides.remove(myside)
    save_missions(missions)
    await interaction.response.send_message("Side "+str(side.name)+" deleted from " + str(mymission.id), ephemeral= True)
    await update_trackedmessages(interaction.client, mymission)
    

@tree.command(
    name="missionadddescription",
    description="Add a description to a mission",
    guild=discord.Object(id=GUILDID)
)
@app_commands.default_permissions(manage_channels=True)
async def missionadddescription(interaction: Interaction, missionid: int, description: str):
    missions = load_missions()
    mymission = None
    for mission in missions:
        if missionid == mission.id:
            mymission = mission
            print("Mission found")
    if mymission == None:
        await interaction.response.send_message("Mission " + str(missionid) + " not found", ephemeral= True)
        return
    mymission.description = description
    save_missions(missions)
    await interaction.response.send_message("Description added to " + str(mymission.id), ephemeral= True)
    await update_trackedmessages(interaction.client, mymission)
    
    
@tree.command(
    name="missionadddivision",
    description="Add a division to a mission",
    guild=discord.Object(id=GUILDID)
)
@app_commands.default_permissions(manage_channels=True)
async def missionadddivision(interaction: Interaction, missionid: int, side: Sides, division: str):
    missions = load_missions()
    mymission = None
    for mission in missions:
        if missionid == mission.id:
            mymission = mission
            print("Mission found")
    if mymission == None:
        await interaction.response.send_message("Mission " + str(missionid) + " not found", ephemeral= True)
        return
    myside = None
    for queryside in mymission.sides:
        if queryside.side is side:
            myside = queryside
            print("side found")
    if myside == None:
        await interaction.response.send_message("Side " + str(side) + " not found", ephemeral= True)
        return
    mydivision = None
    for querydivision in myside.divisions:
        if querydivision.name == division:
            mydivision = querydivision
    if mydivision != None:
        await interaction.response.send_message("Division " + division + " already exists", ephemeral= True)
        return
    newdivision = Division()
    newdivision.name = division
    newdivision.subdivisions = []
    myside.divisions.append(newdivision)
    save_missions(missions)
    await interaction.response.send_message("Division "+division+" added to " + str(mymission.id), ephemeral= True)
    await update_trackedmessages(interaction.client, mymission)
    
    
@tree.command(
    name="missiondeletedivision",
    description="Delete a division from a mission",
    guild=discord.Object(id=GUILDID)
)
@app_commands.default_permissions(manage_channels=True)
async def missiondeletedivision(interaction: Interaction, missionid: int, side: Sides, division: str):
    missions = load_missions()
    mymission = None
    for mission in missions:
        if missionid == mission.id:
            mymission = mission
            print("Mission found")
    if mymission == None:
        await interaction.response.send_message("Mission " + str(missionid) + " not found", ephemeral= True)
        return
    myside = None
    for queryside in mymission.sides:
        if queryside.side is side:
            myside = queryside
            print("side found")
    if myside == None:
        await interaction.response.send_message("Side " + str(side) + " not found", ephemeral= True)
        return
    mydivision = None
    for querydivision in myside.divisions:
        if querydivision.name == division:
            mydivision = querydivision
    if mydivision == None:
        await interaction.response.send_message("Division " + division + " not found", ephemeral= True)
        return
    myside.divisions.remove(mydivision)
    save_missions(missions)
    await interaction.response.send_message("Division "+division+" deleted from " + str(mymission.id), ephemeral= True)
    await update_trackedmessages(interaction.client, mymission)
    
    
@tree.command(
    name="missionaddsubdivision",
    description="Add a division to a mission",
    guild=discord.Object(id=GUILDID)
)
@app_commands.default_permissions(manage_channels=True)
async def missionaddsubdivision(interaction: Interaction, missionid: int, side: Sides, division: str, subdivision: str):
    missions = load_missions()
    mymission = None
    for mission in missions:
        if missionid == mission.id:
            mymission = mission
            print("Mission found")
    if mymission == None:
        await interaction.response.send_message("Mission " + str(missionid) + " not found", ephemeral= True)
        return
    myside = None
    for queryside in mymission.sides:
        if queryside.side is side:
            myside = queryside
            print("side found")
    if myside == None:
        await interaction.response.send_message("Side " + str(side) + " not found", ephemeral= True)
        return
    mydivision = None
    for querydivision in myside.divisions:
        if querydivision.name == division:
            mydivision = querydivision
    if mydivision == None:
        await interaction.response.send_message("Division " + division + " not found", ephemeral= True)
        return
    mysubdivision = None
    for querysubdivision in mydivision.subdivisions:
        if querysubdivision.name == subdivision:
            mysubdivision = querysubdivision
    if mysubdivision != None:
        await interaction.response.send_message("SubDivision " + subdivision + " already exists", ephemeral= True)
        return
    newsubdivision = Subdivision()
    newsubdivision.name = subdivision
    newsubdivision.roles = []
    mydivision.subdivisions.append(newsubdivision)
    save_missions(missions)
    await interaction.response.send_message("Subdivision "+subdivision+" added to " + str(mymission.id), ephemeral= True)
    await update_trackedmessages(interaction.client, mymission)
    

@tree.command(
    name="missiondeletesubdivision",
    description="Delete a division to a mission",
    guild=discord.Object(id=GUILDID)
)
@app_commands.default_permissions(manage_channels=True)
async def missiondeletesubdivision(interaction: Interaction, missionid: int, side: Sides, division: str, subdivision: str):
    missions = load_missions()
    mymission = None
    for mission in missions:
        if missionid == mission.id:
            mymission = mission
            print("Mission found")
    if mymission == None:
        await interaction.response.send_message("Mission " + str(missionid) + " not found", ephemeral= True)
        return
    myside = None
    for queryside in mymission.sides:
        if queryside.side is side:
            myside = queryside
            print("side found")
    if myside == None:
        await interaction.response.send_message("Side " + str(side) + " not found", ephemeral= True)
        return
    mydivision = None
    for querydivision in myside.divisions:
        if querydivision.name == division:
            mydivision = querydivision
    if mydivision == None:
        await interaction.response.send_message("Division " + division + " not found", ephemeral= True)
        return
    mysubdivision = None
    for querysubdivision in mydivision.subdivisions:
        if querysubdivision.name == subdivision:
            mysubdivision = querysubdivision
    if mysubdivision == None:
        await interaction.response.send_message("SubDivision " + subdivision + " not found", ephemeral= True)
        return
    mydivision.subdivisions.remove(mysubdivision)
    save_missions(missions)
    await interaction.response.send_message("Subdivision "+subdivision+" deleted from " + str(mymission.id), ephemeral= True)
    await update_trackedmessages(interaction.client, mymission)
    

    
@tree.command(
    name="missionaddrole",
    description="Add a role to a mission",
    guild=discord.Object(id=GUILDID)
)
@app_commands.default_permissions(manage_channels=True)
async def missionaddrole(interaction: Interaction, missionid: int, side: Sides, division: str, subdivision: str, role: Roles):
    missions = load_missions()
    mymission = None
    for mission in missions:
        if missionid == mission.id:
            mymission = mission
            print("Mission found")
    if mymission == None:
        await interaction.response.send_message("Mission " + str(missionid) + " not found", ephemeral= True)
        return
    myside = None
    for queryside in mymission.sides:
        if queryside.side is side:
            myside = queryside
            print("side found")
    if myside == None:
        myside = Side()
        myside.side = side
        myside.divisions = []
        mymission.sides.append(myside)
    mydivision = None
    for querydivision in myside.divisions:
        if querydivision.name == division:
            mydivision = querydivision
    if mydivision == None:
        mydivision = Division()
        mydivision.name = division
        mydivision.subdivisions = []
        myside.divisions.append(mydivision)
    mysubdivision = None
    for querysubdivision in mydivision.subdivisions:
        if querysubdivision.name == subdivision:
            mysubdivision = querysubdivision
    if mysubdivision == None:
        mysubdivision = Subdivision()
        mysubdivision.name = subdivision
        mysubdivision.roles = []
        mydivision.subdivisions.append(mysubdivision)
    mysubdivision.roles.append(role)
    save_missions(missions)
    await interaction.response.send_message("Role "+str(role.name)+" added to " + str(mymission.id), ephemeral= True)
    await update_trackedmessages(interaction.client, mymission)
    
    
@tree.command(
    name="missiondeleterole",
    description="Delete a role to a mission",
    guild=discord.Object(id=GUILDID)
)
@app_commands.default_permissions(manage_channels=True)
async def missiondeleterole(interaction: Interaction, missionid: int, side: Sides, division: str, subdivision: str, role: Roles):
    missions = load_missions()
    mymission = None
    for mission in missions:
        if missionid == mission.id:
            mymission = mission
            print("Mission found")
    if mymission == None:
        await interaction.response.send_message("Mission " + str(missionid) + " not found", ephemeral= True)
        return
    myside = None
    for queryside in mymission.sides:
        if queryside.side is side:
            myside = queryside
            print("side found")
    if myside == None:
        await interaction.response.send_message("Side " + str(side) + " not found", ephemeral= True)
        return
    mydivision = None
    for querydivision in myside.divisions:
        if querydivision.name == division:
            mydivision = querydivision
    if mydivision == None:
        await interaction.response.send_message("Division " + division + " not found", ephemeral= True)
        return
    mysubdivision = None
    for querysubdivision in mydivision.subdivisions:
        if querysubdivision.name == subdivision:
            mysubdivision = querysubdivision
    if mysubdivision == None:
        await interaction.response.send_message("SubDivision " + subdivision + " not found", ephemeral= True)
        return
    mysubdivision.roles.remove(role)
    save_missions(missions)
    await interaction.response.send_message("Role "+str(role.name)+" deleted from " + str(mymission.id), ephemeral= True)
    await update_trackedmessages(interaction.client, mymission)
    
    
@tree.command(
    name="missionshow",
    description="Show a mission",
    guild=discord.Object(id=GUILDID)
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
    myembed.add_field(name="Description",value=mymission.description,inline=False)
    myembed2=discord.Embed(
        title= "Composition",
        color = Colour.dark_orange()
    )
    for side in mymission.sides:
        tekst = "```"        
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
    guild=discord.Object(id=GUILDID)
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
    guild=discord.Object(id=GUILDID)
)
async def respond(interaction: Interaction, rsvp: Response, side: Sides, primaryrole:  Roles, secondaryrole: typing.Optional[Roles], tertiaryrole: typing.Optional[Roles], missionid: typing.Optional[int]):
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
        if(mission.date > datetime.now()) or mission.id == missionid:
            mymission = mission
    if mymission is None:
        myembed=discord.Embed(
            title= "No ops currently active",
            color = Colour.dark_orange()
        )        
        await interaction.response.send_message( embed= myembed, ephemeral= True  )
        return
    primaryroleisinmission = False
    secondaryroleisinmission = secondaryrole == None
    tertiaryroleisinmission = tertiaryrole == None
    for forside in mymission.sides:
        for division in forside.divisions:
            for subdivision in division.subdivisions:
                for role in subdivision.roles:
                    if primaryrole == role:
                        primaryroleisinmission = True
                    if secondaryrole == role:
                        secondaryroleisinmission = True
                    if tertiaryrole == role:
                        tertiaryroleisinmission = True
    if not primaryroleisinmission or not secondaryroleisinmission or not tertiaryroleisinmission:
        title = ""
        if not primaryroleisinmission:
            title = title + str(primaryrole) + " "
        if not secondaryroleisinmission:
            title = title + str(secondaryrole) + " "
        if not tertiaryroleisinmission:
            title = title + str(tertiaryrole) + " "
        title = title + "is not available for this mission."
        myembed=discord.Embed(
            title= title,
            color = Colour.dark_orange()
        )        
        await interaction.response.send_message( embed= myembed, ephemeral= True  )
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
    
    await update_trackedmessages(interaction.client, mymission)
    
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
    guild=discord.Object(id=GUILDID)
)
async def armabot(interaction: Interaction):
    myembed=discord.Embed(
        title= "ARMA Bot Current commands",
        color = Colour.dark_orange()
    )        
    myembed.add_field(name="/missioncreate",value="Creates a new mission", inline=False)
    myembed.add_field(name="/missionpost",value="Creates the mission post", inline=False)
    myembed.add_field(name="/missionadddescription",value="Adds a description to a created mission", inline=False)
    myembed.add_field(name="/missionaddside",value="Adds a side (e.g. Blufor) to a created mission", inline=False)
    myembed.add_field(name="/missiondeleteside",value="Deletes a side (e.g. Blufor) from a created mission", inline=False)
    myembed.add_field(name="/missionadddivision",value="Adds a division to a side for a created mission", inline=False)
    myembed.add_field(name="/missiondeletedivision",value="Deletes a division to a side from a created mission", inline=False)
    myembed.add_field(name="/missionaddsubdivision",value="Adds a subdivision to a division for a created mission", inline=False)
    myembed.add_field(name="/missiondeletesubdivision",value="Deletes a subdivision to a division from a created mission", inline=False)
    myembed.add_field(name="/missionaddrole",value="Adds a role to a subdivision for a created mission", inline=False)
    myembed.add_field(name="/missiondeleterole",value="Deletes a role to a subdivision from a created mission", inline=False)
    myembed.add_field(name="/missionshow",value="Show a mission. Be aware that this does not get automatically updated (yet)", inline=False)
    myembed.add_field(name="/missionlist",value="Get a list of all missions in the database", inline=False)
    myembed.add_field(name="/respond",value="Allows players to respond to the mission. This assumes there's only 1 mission in the future.", inline=False)
    myembed2=discord.Embed(
        title= "ARMA Bot To Do List",
        color = Colour.dark_orange()
    )        
    myembed2.add_field(name="",value="It doesn't work correctly if there are multiple future missions", inline=False)
    myembed2.add_field(name="",value="Investigate the possibility of adding multiple roles in the same /missionaddrole command, so it becomes less tedious to set up", inline=False)
    myembed2.add_field(name="",value="I have an idea to add a suggested setup, based on primaries/secondaries/tertiaries", inline=False)
    myembed2.add_field(name="",value="You should be able to edit mission names, op names etc.", inline=False)
    embedList = []
    embedList.append(myembed)
    embedList.append(myembed2)
    await interaction.response.send_message( embeds= embedList)


@client.event
async def on_ready():
    await tree.sync(guild=discord.Object(id=GUILDID))
    scheduler.start()
    print("Ready!")

client.run(TOKEN)
