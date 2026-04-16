import { Selector } from "testcafe";

const email = process.env.TESTING_EMAIL;
const password = process.env.TESTING_PASSWORD;

fixture("Login Tests").page("https://fly.io/apps/m2c-filmjournal-client");

test("Login", async t => {
    await t
        .typeText(Selector("#email"), email)
        .typeText(Selector("#password"), password)
        .pressKey("enter")
});
