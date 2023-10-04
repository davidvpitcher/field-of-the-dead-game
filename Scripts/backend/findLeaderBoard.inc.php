<?php
// findLeaderBoard.inc.php

const SUCCESS_CODE = '0';
const QUERY_EMPTY_CODE = '1';
const MISSING_POST_VAR_CODE = '2';
const UNKNOWN_ERROR_CODE = '7';

if (!isset($_POST["code1234"]) || !isset($_POST["player_name"])) {
    echo MISSING_POST_VAR_CODE;
    exit();
}

$unity = true;
$login = true;
require_once '../../keys/storage.php';

try {
    $pdo->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);

    $sql = "SELECT id, username, kills FROM players ORDER BY kills DESC, username ASC LIMIT 50";
    $result = $pdo->query($sql);

    if ($result->rowCount() <= 0) {
        echo QUERY_EMPTY_CODE;
        exit();
    }

    $responseString = SUCCESS_CODE;
    while ($row = $result->fetch(PDO::FETCH_ASSOC)) {
        $responseString .= "\t" . $row["id"] . "\t" . $row["username"] . "\t" . $row["kills"];
    }

    // Fetch player's own stats
    $player_username = $_POST["player_name"];

    $sqlRank = "SELECT COUNT(*) as rank FROM players p1 WHERE p1.kills >= (SELECT p2.kills FROM players p2 WHERE p2.username = :username)";
    $sqlKills = "SELECT kills FROM players WHERE username = :username";

    $stmtRank = $pdo->prepare($sqlRank);
    $stmtKills = $pdo->prepare($sqlKills);

    $stmtRank->bindParam(':username', $player_username, PDO::PARAM_STR);
    $stmtKills->bindParam(':username', $player_username, PDO::PARAM_STR);

    $stmtRank->execute();
    $stmtKills->execute();

    $rowRank = $stmtRank->fetch(PDO::FETCH_ASSOC);
    $rowKills = $stmtKills->fetch(PDO::FETCH_ASSOC);

    // Append player's details
    $responseString .= "\t" . $rowRank["rank"] . "\t" . $player_username . "\t" . $rowKills["kills"];

    echo $responseString;
    exit();

} catch (PDOException $e) {
    error_log("Database error: " . $e->getMessage());
    echo UNKNOWN_ERROR_CODE;
    exit();
}
?>
