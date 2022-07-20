const WebSocket = require('ws');
const fs = require('fs');
const path = require('path')
const wss = new WebSocket.Server({port: 7075});
let allScenesJSON = JSON.parse(fs.readFileSync(require('path').resolve(__dirname, '.')+"/movieDataIt.json"));
let chosedScenesJSON = JSON.parse(fs.readFileSync(require('path').resolve(__dirname, '.')+"/chosedScenes.json"));
let everReadNumber = false;
let numberAnalysis = 0;
wss.on('connection', (ws)=>{
    console.log("new client connected")
    if(!everReadNumber){
        countNumberOfAnalysis();
    }
    /**
     * [messageCode])(opt)[JSON]
     * messageCode:
     *       1: retrive all movie scenes json
     *       2: retrive choosed movie scenes
     *       3: save choosed movie scenes
     *       4: save form result
     */
    ws.on('message',(messageAsString)=>{
        let splits = String(messageAsString).split(")");
        let messageCode = splits[0];
        switch(messageCode){
            case "1":
                ws.send(JSON.stringify(allScenesJSON));
                break;
            case "2":
                ws.send(JSON.stringify(chosedScenesJSON));
                break;
            case "3":
                var jsValue = JSON.parse(splits[1]);
                fs.writeFile(path.resolve(__dirname, '.')+"/chosedScenes.json",JSON.stringify(jsValue,null,'\t'), (err) => {
                    if (err)
                      console.log(err);
                    else {
                      chosedScenesJSON = JSON.parse(fs.readFileSync(path.resolve(__dirname, '.')+"/chosedScenes.json","utf-8"));
                      ws.send("File written successfully\n");
                    }
                });
                break;
            case "4":
                var jsValue = JSON.parse(splits[1]);
                fs.open(path.resolve(__dirname, './statistics')+"/"+jsValue[0]["sessionID"]+".json", 'w', function (err, file) {
                    if (err) throw err;
                    console.log('Saved!');
                  }); 
                fs.writeFile(path.resolve(__dirname, './statistics')+"/"+jsValue[0]["sessionID"]+".json",JSON.stringify(jsValue,null,'\t'),function (err) {
                    if (err) throw err;
                    console.log('Saved!');
                });
                numberAnalysis++;
                ws.send("Perfect");
                break;
            default:
                ws.send("Message code not correct")
        }
    });

    ws.on("close", () => {
        console.log("client disconnect")
    })
    
    function countNumberOfAnalysis(){
        fs.readdir(path.resolve(__dirname, './statistics'), function (err, files) {
            if (err) {
                return console.log('Unable to scan directory: ' + err);
            } 
            numberAnalysis = files.length;
        });
        everReadNumber = true;
    }
});



