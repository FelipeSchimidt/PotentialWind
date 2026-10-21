const { setWorldConstructor, Before, After } = require('@cucumber/cucumber');
const axios = require('axios');

class CustomWorld {
  constructor() {
    this.baseUrl = process.env.API_URL || 'http://localhost:5000';
    this.client = axios.create({
      baseURL: this.baseUrl,
      validateStatus: () => true
    });
    this.response = null;
    this.contextData = {};
  }
}

setWorldConstructor(CustomWorld);
