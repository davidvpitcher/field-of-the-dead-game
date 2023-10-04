<?php
// login.php

// Constants for response codes
const MISSING_POST_DATA = "NO POST";
const QUERY_FAILED = "2: name check query failed";
const USER_ALREADY_LOGGED_IN_OR_DOESNT_EXIST = "5: failed to login, u already logged in or no user";
const INCORRECT_PASSWORD = "6: incorrect password";

$unity = true;
$login = true;
require_once '../../keys/storage.php';

if (isset($_POST["username"]) && isset($_POST["userpassword"])) {
    try {
        $pdo->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);

        $username = $_POST["username"];
        $password = $_POST["userpassword"];

        $stmt = $pdo->prepare("SELECT id, username, salt, hash, kills, funds FROM players WHERE username = :username");
        $stmt->bindParam(':username', $username, PDO::PARAM_STR);
        $stmt->execute();

        if ($stmt->rowCount() != 1) {
            echo USER_ALREADY_LOGGED_IN_OR_DOESNT_EXIST;
            exit();
        }

        $existinginfo = $stmt->fetch(PDO::FETCH_ASSOC);
        $salt = $existinginfo["salt"];
        $hash = $existinginfo["hash"];

        $loginhash = crypt($password, $salt);
        if ($hash != $loginhash) {
            echo INCORRECT_PASSWORD;
            exit();
        }

        echo "0\t" . $existinginfo["kills"] . "\t" . $existinginfo["funds"] . "\t" . $existinginfo["id"];
        
    } catch (PDOException $e) {
        error_log("Database error: " . $e->getMessage());
        echo QUERY_FAILED;
        exit();
    }
} else {
    echo MISSING_POST_DATA;
    exit();
}
?>
