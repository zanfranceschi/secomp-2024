import scala.concurrent.duration._

import scala.util.Random

import io.gatling.core.Predef._
import io.gatling.http.Predef._

import java.util.UUID

class SecompSimulation
  extends Simulation {

  def uuid() = UUID.randomUUID.toString
  def randomValorTransacao() = BigDecimal(Random.between(0.01, 10000.0 + 1.0))
    .setScale(2, BigDecimal.RoundingMode.HALF_UP)

  val httpProtocol = http
    .baseUrl("http://localhost:9999")
    .userAgentHeader("SECOMP - 2024")

  val transferencias = scenario("Solicitação de Transferência")
    .exec {s =>
      val clienteIdDe = uuid()
      val clienteIdPara = uuid()
      val valor = randomValorTransacao()
      val payload = s"""{"valor": ${valor}, "clienteIdDe": "${clienteIdDe}", "clienteIdPara": "${clienteIdPara}"}"""
      val session = s.setAll(Map("payload" -> payload))
      session
    }
    .exec(
      http("transferências")
      .post("/transferencias").body(StringBody("#{payload}"))
      .header("content-type", "application/json")
      .check(status.in(201, 202))
    )

  setUp(
    transferencias.inject(
      rampUsersPerSec(1).to(1000).during(3.minutes)
    )
  ).protocols(httpProtocol)
}
