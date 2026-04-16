import { Selector, ClientFunction } from "testcafe";

const email = process.env.TESTING_EMAIL;
const password = process.env.TESTING_PASSWORD;

const getUrl = ClientFunction(() => window.location.href);

fixture("Login Tests").page("https://m2c-filmjournal-client.fly.dev/");

test("Login redirects to dashboard", async t => {
    await t
        .typeText(Selector("#email"), email)
        .typeText(Selector("#password"), password)
        .pressKey("enter")
        .expect(getUrl()).contains("/dashboard", "Should redirect to dashboard after login")
        .click(Selector('img').nth(0))
        .expect(getUrl()).contains("/movie", "Should redirect to the first available movie")
});
