
const Path = require("path");
const Express = require("express");
const BodyParser = require("body-parser");
const Data = require("./data.js");

const PORT = 8080;
const PUBLIC_PATH = Path.join(__dirname, "../../output");
const DATA_PATH = Path.join(__dirname, "../../data/data.json");

const app = Express();

app.use(
    Express.static(PUBLIC_PATH, {
        index: "index.html"
    }),
    BodyParser.json()
)

// Prevent cache over JSON response
app.set("etag", false)

app.get("/api/talk/:id", (req, res) => {
    const id = req.param("id");
    sendJson(res, Data.get()[id]);
});

app.post("/api/vote/:talkid/:takeid", (req, res) => {
    const talkid = req.param("talkid");
    const takeid = req.param("takeid");
    const take = data[talkid].takes[takeid];
    take.votes++;
    saveData();
    sendJson(res, take.votes);
});


Data.init(DATA_PATH);
app.listen(PORT, () => {
    console.log("Listening on port", PORT);
});

function sendJson(res, data) {
    res.setHeader("Content-Type", "application/json");
    res.send(JSON.stringify(data));
}
