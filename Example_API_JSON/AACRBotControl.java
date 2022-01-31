package com.CRControl;

import java.io.BufferedReader;
import java.io.DataOutputStream;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.net.HttpURLConnection;
import java.net.URL;
import org.json.simple.JSONArray;
import org.json.simple.JSONObject;
import org.json.simple.parser.JSONParser;

// "{\"filter\": {\"operator\": \"and\", \"operands\": [
// {\"operator\": \"eq\",\"value\": \"" + JObject.Parse(GetDeviceID(deviceName))["list"][0]["id"].ToString() + "\", \"field\": \"deviceId\"}
// , {\"operator\": \"eq\",\"value\": \"" + JObject.Parse(GetBotInfo(botName))["list"][0]["id"].ToString() + "\", \"field\": \"fileId\"}
// , {\"operator\": \"eq\",\"value\": \"" + automationId + "\", \"field\": \"automationId\"}
// ]}}"

public class AACRBotControl {
    
    /**
     * monitorBot
     * Returns Status,Progress
     */

    public static String monitorBot(String AACRUrl, String AAUsername, String AAAPIKey, String BotName, String DeviceName, String AutomationId){
        HttpURLConnection connection = null;
        
	/**
        * System.out.println("AA CR URL: "+AACRUrl);
        * System.out.println("AAUsername: "+AAUsername);
        * System.out.println("AA API Key: "+AAAPIKey);
        * System.out.println("Bot name: "+BotName);
        * System.out.println("Device Name: "+DeviceName);
        * System.out.println("Automation id: "+AutomationId);
	*/
	
        //get auth token
        String authToken = getAuthToken(AACRUrl,AAUsername,AAAPIKey);
        //get bot id
        String BotID = getBotID(AACRUrl,AAUsername,AAAPIKey,BotName,authToken);
        //get device id
        String DeviceID = getDeviceID(AACRUrl,AAUsername,AAAPIKey,DeviceName,authToken);
        //setup location for deploy
        AACRUrl = AACRUrl+"/v2/activity/list";
        
        //set up request body
        String httpRequestBody = "{\"filter\": {\"operator\": \"and\", \"operands\": [{\"operator\": \"eq\",\"value\": \""+DeviceID+"\", \"field\": \"deviceId\"}, {\"operator\": \"eq\", \"value\": \""+BotID		+"\", \"field\": \"fileId\"}, {\"operator\": \"eq\", \"value\": \""+AutomationId+"\",\"field\": \"automationId\"}]}}";
        //System.out.println(httpRequestBody);
        
        try{
            //setup connection
            URL url = new URL(AACRUrl);
            connection = (HttpURLConnection) url.openConnection();
            connection.setRequestMethod("POST");
            connection.setRequestProperty("X-Authorization",authToken);
            connection.setUseCaches(false);
            connection.setDoOutput(true);
            //connection.setRequestProperty("Content-Length", Integer.toString(httpRequestBody.getBytes().length));
            //connection.setRequestProperty("Content-Language", "en-US");  
            
            
            //send request and body
            DataOutputStream wr = new DataOutputStream(connection.getOutputStream());
            wr.writeBytes(httpRequestBody);
            wr.close();
            
            //get response
            InputStream is = connection.getInputStream();
            BufferedReader rd = new BufferedReader(new InputStreamReader(is));
            StringBuilder response = new StringBuilder();
            String line;
            while((line=rd.readLine())!=null){
                response.append(line);
                response.append('\n');
            }
            rd.close();
            
            //System.out.print(response.toString());
            
            //get json status and progress
            JSONParser parser = new JSONParser();
            Object obj = parser.parse(response.toString());
            JSONObject json1 = (JSONObject) obj;
            JSONArray array = (JSONArray) json1.get("list");
            JSONObject json2 = (JSONObject) array.get(0);
            return json2.get("status").toString()+","+json2.get("progress").toString();
            //return response.toString();

        } catch (Exception e){
            e.printStackTrace();
            return e.getMessage();
        } finally{
            if(connection!=null){
                connection.disconnect();
            }
        }
    }

    /**
     * deployBot
     * 
     * BotInput requires a JSON format for input.
     * Format: {"VARIABLE": {"TYPE": "VALUE"}, "VARIABLE2": {"TYPE": "VALUE"}}
     * 
     */

    public static String deployBot(String AACRUrl, String AAUsername, String AAAPIKey, String BotName, String DeviceName, String BotInput){

        HttpURLConnection connection = null;

	/**
        * System.out.println("AACRUrl: "+AACRUrl);
        * System.out.println("AAUsername: "+AAUsername);
        * System.out.println("AAAPIKey: "+AAAPIKey);
        * System.out.println("BotName: "+BotName);
        * System.out.println("DeviceName: "+DeviceName);
        * System.out.println("BotInput: "+BotInput);
	*/

        //get auth token
        String authToken = getAuthToken(AACRUrl,AAUsername,AAAPIKey);
        //System.out.println("authToken: "+authToken);

        //get bot id
        String BotID = getBotID(AACRUrl,AAUsername,AAAPIKey,BotName,authToken);
        //System.out.println("BotID: "+BotID);

        //get device id
        String DeviceID = getDeviceID(AACRUrl,AAUsername,AAAPIKey,DeviceName,authToken);
        //System.out.println("DeviceID: "+DeviceID);

        //parse input values
        JSONObject json1 = new JSONObject();
        try{
            String[] strArray = BotInput.split("\\|");
            for (int i = 0;i<strArray.length;i=i+1){
                System.out.println(strArray[i]);
            }
            int strArray_Length = strArray.length;
            for (int i = 0;i<strArray_Length;i=i+3){
                JSONObject json2 = new JSONObject();
                json2.put(strArray[i+1],strArray[i+2]);
                json1.put(strArray[i], json2);
            }
        } catch (Exception e){
            return e.getMessage();
        }
	//System.out.println("json input = "+json1.toString());
        //end parse input values

        //setup location for deploy
        AACRUrl = AACRUrl+"/v2/automations/deploy";
        
        //setup request body
        //{"VARIABLE": {"TYPE": "VALUE"}, "VARIABLE2": {"TYPE": "VALUE"}}
        String httpRequestBody;
        if((BotInput == null)||(BotInput.equals(""))){
            httpRequestBody = "{\"fileId\": \""+BotID+"\", \"deviceIds\": [\""+DeviceID+"\"], \"runWithRdp\": false}";
        } else {
            httpRequestBody = "{\"fileId\": \""+BotID+"\", \"deviceIds\": [\""+DeviceID+"\"], \"runWithRdp\": false, \"botVariables\": "+json1.toString()+"}";
        }
        
        //System.out.println(httpRequestBody);
        try{
            //setup connection
            URL url = new URL(AACRUrl);
            connection = (HttpURLConnection) url.openConnection();
            connection.setRequestMethod("POST");
            connection.setRequestProperty("X-Authorization",authToken);
            connection.setUseCaches(false);
            connection.setDoOutput(true);
            //connection.setRequestProperty("Content-Length", Integer.toString(httpRequestBody.getBytes().length));
            //connection.setRequestProperty("Content-Language", "en-US");  
            
            
            //send request and body
            DataOutputStream wr = new DataOutputStream(connection.getOutputStream());
            wr.writeBytes(httpRequestBody);
            wr.close();
            
            //get response
            InputStream is = connection.getInputStream();
            BufferedReader rd = new BufferedReader(new InputStreamReader(is));
            StringBuilder response = new StringBuilder();
            String line;
            while((line=rd.readLine())!=null){
                response.append(line);
                response.append('\n');
            }
            rd.close();
            
            //get just the auto id
            JSONParser parser = new JSONParser();
            Object obj = parser.parse(response.toString());
            JSONObject json = (JSONObject)obj;
            return json.get("automationId").toString();
            //return response.toString();
        } catch (Exception e){
            //e.printStackTrace();
            return e.getMessage();
        } finally{
            if(connection!=null){
                connection.disconnect();
            }
        }
    }
    
    private static String getDeviceID(String AACRUrl, String AAUsername, String AAAPIKey, String DeviceName, String authToken){

        HttpURLConnection connection = null;
        
        //setup location for deploy
        AACRUrl = AACRUrl+"/v2/devices/list";
        
        //setup request body
        String httpRequestBody = "{\"filter\": {\"operator\": \"substring\", \"field\": \"hostName\", \"value\": \""+DeviceName+"\"}}";
        //System.out.println("getDeviceID body: "+httpRequestBody);
        
        try{
            //setup connection
            URL url = new URL(AACRUrl);
            connection = (HttpURLConnection) url.openConnection();
            connection.setRequestMethod("POST");
            connection.setRequestProperty("X-Authorization",authToken);
            connection.setUseCaches(false);
            connection.setDoOutput(true);
            //connection.setRequestProperty("Content-Length", Integer.toString(httpRequestBody.getBytes().length));
            //connection.setRequestProperty("Content-Language", "en-US");  
            
            
            //send request and body
            DataOutputStream wr = new DataOutputStream(connection.getOutputStream());
            wr.writeBytes(httpRequestBody);
            wr.close();
            
            //get response
            InputStream is = connection.getInputStream();
            BufferedReader rd = new BufferedReader(new InputStreamReader(is));
            StringBuilder response = new StringBuilder();
            String line;
            while((line=rd.readLine())!=null){
                response.append(line);
                response.append('\n');
            }
            rd.close();
            
            //get just the id
            JSONParser parser = new JSONParser();
            Object obj = parser.parse(response.toString());
            JSONObject json = (JSONObject)obj;
            JSONArray array = (JSONArray)json.get("list");
            json = (JSONObject)array.get(0);
            
            return json.get("id").toString();
            //return response.toString();
        } catch (Exception e){
            e.printStackTrace();
            return e.getMessage();
        } finally{
            if(connection!=null){
                connection.disconnect();
            }
        }
    }
    
    private static String getBotID(String AACRUrl, String AAUsername, String AAAPIKey, String BotName, String authToken){
        HttpURLConnection connection = null;
        
        //setup location for deploy
        AACRUrl = AACRUrl+"/v2/repository/file/list";
        
        //setup request body
        String httpRequestBody = "{\"filter\": {\"operator\": \"substring\", \"field\": \"name\", \"value\": \""+BotName+"\"}}";
        //System.out.println("getBotID body: "+httpRequestBody);
        try{
            //setup connection
            URL url = new URL(AACRUrl);
            connection = (HttpURLConnection) url.openConnection();
            connection.setRequestMethod("POST");
            connection.setRequestProperty("X-Authorization",authToken);
            connection.setUseCaches(false);
            connection.setDoOutput(true);
            //connection.setRequestProperty("Content-Length", Integer.toString(httpRequestBody.getBytes().length));
            //connection.setRequestProperty("Content-Language", "en-US");  
            
            
            //send request and body
            DataOutputStream wr = new DataOutputStream(connection.getOutputStream());
            wr.writeBytes(httpRequestBody);
            wr.close();
            
            //get response
            InputStream is = connection.getInputStream();
            BufferedReader rd = new BufferedReader(new InputStreamReader(is));
            StringBuilder response = new StringBuilder();
            String line;
            while((line=rd.readLine())!=null){
                response.append(line);
                response.append('\n');
            }
            rd.close();
            
            //get just the id
            JSONParser parser = new JSONParser();
            Object obj = parser.parse(response.toString());
            JSONObject json = (JSONObject)obj;
            JSONArray array = (JSONArray)json.get("list");
            json = (JSONObject)array.get(0);
            
            return json.get("id").toString();
        } catch (Exception e){
            //e.printStackTrace();
            return e.getMessage();
        } finally{
            if(connection!=null){
                connection.disconnect();
            }
        }
    }
    
    private static String getAuthToken(String AACRUrl, String AAUsername, String AAAPIKey){
        HttpURLConnection connection = null;
        //setup location for authentication
        AACRUrl = AACRUrl+"/v1/authentication";
        //setup request body
        String httpRequestBody = "{\"username\": \"mhco\\\\"+AAUsername+"\",\"apiKey\": \""+AAAPIKey+"\"}";
        
        try{
            //setup connection
            URL url = new URL(AACRUrl);
            connection = (HttpURLConnection) url.openConnection();
            connection.setRequestMethod("POST");
            connection.setRequestProperty("Content-Type","application/json");
            connection.setUseCaches(false);
            connection.setDoOutput(true);
            connection.setRequestProperty("Content-Length", Integer.toString(httpRequestBody.getBytes().length));
            connection.setRequestProperty("Content-Language", "en-US");  
            
            //send request and body
            DataOutputStream wr = new DataOutputStream(connection.getOutputStream());
            wr.writeBytes(httpRequestBody);
            wr.close();
            
            //get response
            InputStream is = connection.getInputStream();
            BufferedReader rd = new BufferedReader(new InputStreamReader(is));
            StringBuilder response = new StringBuilder();
            String line;
            while((line=rd.readLine())!=null){
                response.append(line);
                response.append('\n');
            }
            rd.close();
            
            //get just the token
            JSONParser parser = new JSONParser();
            Object obj = parser.parse(response.toString());
            JSONObject json = (JSONObject)obj;
            
            return json.get("token").toString();
        } catch (Exception e){
            e.printStackTrace();
            return e.getMessage();
        } finally{
            if(connection!=null){
                connection.disconnect();
            }
        }
    }
    
}
