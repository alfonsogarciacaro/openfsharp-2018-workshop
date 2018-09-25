
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
        const talk = getById(talks, talkid);
        talk.takeAways.push(take);
        return take;
    });
    sendJson(res, take);
});

app.post("/api/vote/:talkid", (req, res) => {
    const talkid = req.param("talkid");
    const take = Data.edit(talks => {
        const takeNew = req.body;
        const talk = getById(talks, talkid);
        const takeOld = getById(talk.takeAways, takeNew.id);
        // Just increase the votes
        takeOld.votes++;
        return takeOld;
    });
    sendJson(res, take);
});


Data.init(DATA_PATH);
app.listen(PORT, () => {
    console.log("Listening on port", PORT);
});

function sendJson(res, data) {
    res.setHeader("Content-Type", "application/json");
    res.send(JSON.stringify(data));
}

function getById(arr, id) {
    for (let x of arr) {
        if (x.id === id) {
            return x;
        }
    }
    return null;
}