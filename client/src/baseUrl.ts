const isProduction = import.meta.env.PROD;
const prod = "https://m2c-filmjournal-server.fly.dev";
const dev = "http://localhost:5107";

export const finalUrl = isProduction ? prod : dev;