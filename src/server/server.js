
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

app.get("/api/talks", (req, res) => {
    sendJson(res, Data.get());
});

app.get("/api/talks/:id", (req, res) => {
    const id = req.param("id");
    sendJson(res, Data.get()[id]);
});

app.post("/api/takeaways/:talkid", (req, res) => {
    const talkid = req.param("talkid");
    const take = Data.edit(talks => {
        const take = req.body;
        for (let talk of talks) {
            if (talk.id === talkid) {
                talk.takeAways.push(take);
                break;
            }
        }
        return take;
    });
    sendJson(res, take);
});

app.post("/api/vote/:talkid/:takeid", (req, res) => {
    const talkid = req.param("talkid");
    const takeid = req.param("takeid");
    const take = Data.edit(d => {
        const take = data[talkid].takes[takeid];
        take.votes++;
        return take;
    });
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
