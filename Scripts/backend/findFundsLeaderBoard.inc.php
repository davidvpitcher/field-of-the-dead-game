<?php
// findFundsLeaderBoard.inc.php

// Constants for response codes
const SUCCESS_CODE = '0';
const QUERY_EMPTY_CODE = '1';
const MISSING_POST_VAR_CODE = '2';
const UNKNOWN_ERROR_CODE = '7';

// Check for the required POST variables
if (!isset($_POST["code1234"]) || !isset($_POST["player_name"])) {
    echo MISSING_POST_VAR_CODE;
    exit();
}

$unity = true;
$login = true;
require_once '../../keys/storage.php';

try {
    $pdo->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);

    // Query for leaderboard based on funds
    $sql = <<<'SQL'
    SELECT id, username, funds 
    FROM players 
    ORDER BY funds DESC, username ASC 
    LIMIT 10
    SQL;

    $result = $pdo->query($sql);

    if ($result->rowCount() <= 0) {
        echo QUERY_EMPTY_CODE;
        exit();
    }

    $responseString = SUCCESS_CODE;
    while ($row = $result->fetch(PDO::FETCH_ASSOC)) {
        $responseString .= "\t" . $row["id"] . "\t" . $row["username"] . "\t" . $row["funds"];
    }

    // Fetch player's own stats
    $player_username = $_POST["player_name"];

    // Query for rank based on funds
    $sqlRank = "SELECT COUNT(*) as rank 
                FROM players p1
                WHERE p1.funds >= (SELECT p2.funds FROM players p2 WHERE p2.username = :username)";

    // Query for player's own funds
    $sqlFunds = "SELECT funds 
                 FROM players
                 WHERE username = :username";

    $stmtRank = $pdo->prepare($sqlRank);
    $stmtFunds = $pdo->prepare($sqlFunds);
    $stmtRank->bindParam(':username', $player_username, PDO::PARAM_STR);
    $stmtFunds->bindParam(':username', $player_username, PDO::PARAM_STR);
    $stmtRank->execute();
    $stmtFunds->execute();

    $rowRank = $stmtRank->fetch(PDO::FETCH_ASSOC);
    $rowFunds = $stmtFunds->fetch(PDO::FETCH_ASSOC);

    // Append player's own rank and funds
    $responseString .= "\t" . $rowRank["rank"] . "\t" . $player_username . "\t" . $rowFunds["funds"];

    echo $responseString;
    exit();

} catch (PDOException $e) {
    error_log("Database error: " . $e->getMessage());
    echo UNKNOWN_ERROR_CODE;
    exit();
}
?>
