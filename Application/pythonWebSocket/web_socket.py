import asyncio
from asyncio import constants
import json
import websockets
 
# create handler for each connection
 
async def handler(websocket, path):
    data = await websocket.recv()
    splits = data.split("=")
    reply = f"Data recieved as:  {data}!"
    await websocket.send(reply)
 
allScenesJSON = ""

with open('/Users/rinorenna/Desktop/EmotionVideoElicitation/NodeSocket/movieData.json','r') as outfile:
    allScene=json.load(outfile,indent=4, separators=(',', ': '))

print(allScene)
start_server = websockets.serve(handler, "localhost", 8000)
 
 
 
asyncio.get_event_loop().run_until_complete(start_server)
 
asyncio.get_event_loop().run_forever()
